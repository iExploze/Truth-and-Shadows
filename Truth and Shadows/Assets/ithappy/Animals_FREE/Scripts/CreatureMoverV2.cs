using System;
using System.Collections;
using System.Collections.Generic;
using TruthAndShadows.InputSystem;
using UnityEditor;
using UnityEngine;

namespace Controller
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Animator))]
    [DisallowMultipleComponent]
    public class CreatureMoverV2 : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField]
        private float m_WalkSpeed = 1f;

        [SerializeField]
        private float m_RunSpeed = 4f;

        [SerializeField, Range(0f, 360f)]
        private float m_RotateSpeed = 90f;

        [SerializeField]
        private Space m_Space = Space.Self;

        [SerializeField]
        private float m_JumpHeight = 5f;

        [Header("Animator")]
        [SerializeField]
        private string m_VerticalID = "Vert";

        [SerializeField]
        private string m_StateID = "State";

        [SerializeField]
        private LookWeight m_LookWeight = new(1f, 0.3f, 0.7f, 1f);

        [Header("Transform Sequences")]
        [SerializeField]
        private List<TransformSet> m_TransformSets = new List<TransformSet>();

        [SerializeField]
        private float m_MovementSpeed = 2f;

        [SerializeField]
        private float m_LookDuration = 1f;

        [SerializeField]
        private float m_FadeDuration = 1f;

        private Transform m_Transform;
        private CharacterController m_Controller;
        private Animator m_Animator;

        private MovementHandler m_Movement;
        private AnimationHandler m_Animation;

        private Vector2 m_Axis;
        private Vector3 m_Target;
        private bool m_IsRun;

        private bool m_IsMoving;

        // Sequence variables
        private bool m_IsInSequence = false;
        private int m_CurrentSetIndex = 0;
        private Renderer[] m_Renderers;
        private Material[] m_OriginalMaterials;

        public Vector2 Axis => m_Axis;
        public Vector3 Target => m_Target;
        public bool IsRun => m_IsRun;

        private void OnValidate()
        {
            m_WalkSpeed = Mathf.Max(m_WalkSpeed, 0f);
            m_RunSpeed = Mathf.Max(m_RunSpeed, m_WalkSpeed);

            m_Movement?.SetStats(
                m_WalkSpeed / 3.6f,
                m_RunSpeed / 3.6f,
                m_RotateSpeed,
                m_Space
            );
        }

        private void Awake()
        {
            m_Transform = transform;
            m_Controller = GetComponent<CharacterController>();
            m_Animator = GetComponent<Animator>();

            m_Movement = new MovementHandler(
                m_Controller,
                m_Transform,
                m_WalkSpeed,
                m_RunSpeed,
                m_RotateSpeed,
                m_Space
            );
            m_Animation = new AnimationHandler(m_Animator, m_VerticalID, m_StateID);

            // Setup rendering for fade effects
            m_Renderers = GetComponentsInChildren<Renderer>();
            m_OriginalMaterials = new Material[m_Renderers.Length];
            for (int i = 0; i < m_Renderers.Length; i++)
            {
                m_OriginalMaterials[i] = m_Renderers[i].material;
            }
            SetAlpha(0f); // Start invisible
        }

        private void Update()
        { 
            // Check for hint button (K key or controller button: B on Xbox, Circle on PS, A on Switch Pro) to start sequence
            if (
                (
                    InputManager.Instance != null && InputManager.Instance.GetHintButtonDown()
                    || (InputManager.Instance == null && Input.GetKeyDown(KeyCode.K))
                ) && !m_IsInSequence
            )
            {
                StartTransformSequence();
                Debug.Log("Starting dog hint sequence");
            }            // Only handle normal movement when not in sequence
            if (!m_IsInSequence)
            {
                m_Movement.Move(
                    Time.deltaTime,
                    in m_Axis,
                    in m_Target,
                    m_IsRun,
                    m_IsMoving,
                    out var animAxis,
                    out var isAir
                );
                m_Animation.Animate(in animAxis, m_IsRun ? 1f : 0f, Time.deltaTime);
            }
        }

        private void OnAnimatorIK()
        {
            m_Animation.AnimateIK(in m_Target, m_LookWeight);
        }

        public void SetInput(in Vector2 axis, in Vector3 target, in bool isRun, in bool isJump)
        {
            m_Axis = axis;
            m_Target = target;
            m_IsRun = isRun;

            if (m_Axis.sqrMagnitude < Mathf.Epsilon)
            {
                m_Axis = Vector2.zero;
                m_IsMoving = false;
            }
            else
            {
                m_Axis = Vector3.ClampMagnitude(m_Axis, 1f);
                m_IsMoving = true;
            }
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (hit.normal.y > m_Controller.stepOffset)
            {
                m_Movement.SetSurface(hit.normal);
            }
        }

        [Serializable]
        private struct LookWeight
        {
            public float weight;
            public float body;
            public float head;
            public float eyes;

            public LookWeight(float weight, float body, float head, float eyes)
            {
                this.weight = weight;
                this.body = body;
                this.head = head;
                this.eyes = eyes;
            }
        }

        #region Handlers
        sealed class MovementHandler
        {
            private readonly CharacterController m_Controller;
            private readonly Transform m_Transform;

            private float m_WalkSpeed;
            private float m_RunSpeed;
            private float m_RotateSpeed;

            private Space m_Space;

            private readonly float m_Luft = 75f;

            private float m_TargetAngle;
            private bool m_IsRotating = false;

            private Vector3 m_Normal;
            private Vector3 m_GravityAcelleration = Physics.gravity;

            private float m_jumpTimer;
            private Vector3 m_LastForward;

            public MovementHandler(
                CharacterController controller,
                Transform transform,
                float walkSpeed,
                float runSpeed,
                float rotateSpeed,
                Space space
            )
            {
                m_Controller = controller;
                m_Transform = transform;

                m_WalkSpeed = walkSpeed;
                m_RunSpeed = runSpeed;
                m_RotateSpeed = rotateSpeed;

                m_Space = space;
            }

            public void SetStats(float walkSpeed, float runSpeed, float rotateSpeed, Space space)
            {
                m_WalkSpeed = walkSpeed;
                m_RunSpeed = runSpeed;
                m_RotateSpeed = rotateSpeed;

                m_Space = space;
            }

            public void SetSurface(in Vector3 normal)
            {
                m_Normal = normal;
            }

            public void Move(
                float deltaTime,
                in Vector2 axis,
                in Vector3 target,
                bool isRun,
                bool isMoving,
                out Vector2 animAxis,
                out bool isAir
            )
            {
                var cameraLook = Vector3.Normalize(target - m_Transform.position);
                var targetForward = m_LastForward;

                ConvertMovement(in axis, in cameraLook, out var movement);
                if (movement.sqrMagnitude > 0.5f)
                {
                    m_LastForward = Vector3.Normalize(movement);
                }

                CaculateGravity(deltaTime, out isAir);
                Displace(deltaTime, in movement, isRun);
                Turn(in targetForward, isMoving);
                UpdateRotation(deltaTime);

                GenAnimationAxis(in movement, out animAxis);
            }

            private void ConvertMovement(
                in Vector2 axis,
                in Vector3 targetForward,
                out Vector3 movement
            )
            {
                Vector3 forward;
                Vector3 right;

                if (m_Space == Space.Self)
                {
                    forward = new Vector3(targetForward.x, 0f, targetForward.z).normalized;
                    right = Vector3.Cross(Vector3.up, forward).normalized;
                }
                else
                {
                    forward = Vector3.forward;
                    right = Vector3.right;
                }

                movement = axis.x * right + axis.y * forward;
                movement = Vector3.ProjectOnPlane(movement, m_Normal);
            }

            private void Displace(float deltaTime, in Vector3 movement, bool isRun)
            {
                Vector3 displacement = (isRun ? m_RunSpeed : m_WalkSpeed) * movement;
                displacement += m_GravityAcelleration;
                displacement *= deltaTime;

                m_Controller.Move(displacement);
            }

            private void CaculateGravity(float deltaTime, out bool isAir)
            {
                m_jumpTimer = Mathf.Max(m_jumpTimer - deltaTime, 0f);

                if (m_Controller.isGrounded)
                {
                    m_GravityAcelleration = Physics.gravity;
                    isAir = false;

                    return;
                }

                isAir = true;

                m_GravityAcelleration += Physics.gravity * deltaTime;
            }

            private void GenAnimationAxis(in Vector3 movement, out Vector2 animAxis)
            {
                if (m_Space == Space.Self)
                {
                    animAxis = new Vector2(
                        Vector3.Dot(movement, m_Transform.right),
                        Vector3.Dot(movement, m_Transform.forward)
                    );
                }
                else
                {
                    animAxis = new Vector2(
                        Vector3.Dot(movement, Vector3.right),
                        Vector3.Dot(movement, Vector3.forward)
                    );
                }
            }

            private void Turn(in Vector3 targetForward, bool isMoving)
            {
                var angle = Vector3.SignedAngle(
                    m_Transform.forward,
                    Vector3.ProjectOnPlane(targetForward, Vector3.up),
                    Vector3.up
                );

                if (!m_IsRotating)
                {
                    if (!isMoving && Mathf.Abs(angle) < m_Luft)
                    {
                        m_IsRotating = false;
                        return;
                    }

                    m_IsRotating = true;
                }

                m_TargetAngle = angle;
            }

            private void UpdateRotation(float deltaTime)
            {
                if (!m_IsRotating)
                {
                    return;
                }

                var rotDelta = m_RotateSpeed * deltaTime;
                if (rotDelta + Mathf.PI * 2f + Mathf.Epsilon >= Mathf.Abs(m_TargetAngle))
                {
                    rotDelta = m_TargetAngle;
                    m_IsRotating = false;
                }
                else
                {
                    rotDelta *= Mathf.Sign(m_TargetAngle);
                }

                m_Transform.Rotate(Vector3.up, rotDelta);
            }
        }

        sealed class AnimationHandler
        {
            private readonly Animator m_Animator;
            private readonly string m_VerticalID;
            private readonly string m_StateID;

            private readonly float k_InputFlow = 4.5f;

            private float m_FlowState;
            private Vector2 m_FlowAxis;

            public AnimationHandler(Animator animator, string verticalID, string stateID)
            {
                m_Animator = animator;
                m_VerticalID = verticalID;
                m_StateID = stateID;
            }

            public void Animate(in Vector2 axis, float state, float deltaTime)
            {
                m_Animator.SetFloat(m_VerticalID, m_FlowAxis.magnitude);
                m_Animator.SetFloat(m_StateID, Mathf.Clamp01(m_FlowState));

                m_FlowAxis = Vector2.ClampMagnitude(
                    m_FlowAxis + k_InputFlow * deltaTime * (axis - m_FlowAxis).normalized,
                    1f
                );
                m_FlowState = Mathf.Clamp01(
                    m_FlowState + k_InputFlow * deltaTime * Mathf.Sign(state - m_FlowState)
                );
            }

            public void AnimateIK(in Vector3 target, in LookWeight lookWeight)
            {
                m_Animator.SetLookAtPosition(target);
                m_Animator.SetLookAtWeight(
                    lookWeight.weight,
                    lookWeight.body,
                    lookWeight.head,
                    lookWeight.eyes
                );
            }
        }
        #endregion

        public void StartTransformSequence()
        {
            if (m_TransformSets.Count == 0)
            {
                Debug.LogWarning("No transform sets available");
                return;
            }

            // If we've reached the end, repeat the last set
            if (m_CurrentSetIndex >= m_TransformSets.Count)
            {
                m_CurrentSetIndex = m_TransformSets.Count - 1;
            }

            StartCoroutine(ExecuteTransformSequence(m_TransformSets[m_CurrentSetIndex]));

            // Only increment if we haven't reached the last set
            if (m_CurrentSetIndex < m_TransformSets.Count - 1)
            {
                m_CurrentSetIndex++;
            }
        }

        private IEnumerator ExecuteTransformSequence(TransformSet transformSet)
        {
            m_IsInSequence = true;

            // IMPORTANT: Ensure creature is invisible before positioning
            SetAlpha(0f);

            // Always position the creature at the first movement position before making visible
            Transform firstMovementTransform = null;

            // Find the first movement position in this set
            for (int i = 0; i < transformSet.transforms.Count; i++)
            {
                var transformData = transformSet.transforms[i];
                if (transformData.type == TransformType.Movement)
                {
                    firstMovementTransform = transformData.transform;
                    break;
                }
            } // If there's a movement position, teleport there while invisible
            if (firstMovementTransform != null)
            {
                Debug.Log(
                    $"Teleporting to first position: {firstMovementTransform.name} at {firstMovementTransform.position}"
                );

                // Disable CharacterController temporarily to ensure position change takes effect
                m_Controller.enabled = false;
                m_Transform.position = firstMovementTransform.position;
                m_Controller.enabled = true;

                Debug.Log($"Position after teleport: {m_Transform.position}");
            }
            else
            {
                Debug.LogWarning("No movement transforms found in this set!");
            } // Wait a frame to ensure position is set
            yield return null;

            // Now fade in at the correct location
            Debug.Log($"About to fade in. Current position: {m_Transform.position}");
            yield return StartCoroutine(FadeCoroutine(0f, 1f));
            Debug.Log($"Faded in. Final position: {m_Transform.position}");

            // Process all transforms in order
            bool isFirstMovement = true;

            for (int i = 0; i < transformSet.transforms.Count; i++)
            {
                var transformData = transformSet.transforms[i];

                if (transformData.type == TransformType.Movement)
                {
                    if (isFirstMovement)
                    {
                        // Skip the first movement since we already teleported there
                        isFirstMovement = false;
                    }
                    else
                    {
                        // Move to subsequent movement positions
                        yield return StartCoroutine(MoveToTransform(transformData.transform));
                    }
                }
                else if (transformData.type == TransformType.Look)
                {
                    // Look at the transform
                    yield return StartCoroutine(
                        LookAtTransform(transformData.transform, m_LookDuration)
                    );
                }
            }

            // Fade out
            yield return StartCoroutine(FadeCoroutine(1f, 0f));

            m_IsInSequence = false;
        }

        private IEnumerator MoveToTransform(Transform target)
        {
            Vector3 startPos = m_Transform.position;
            Vector3 targetPos = target.position;
            float distance = Vector3.Distance(startPos, targetPos);
            float duration = distance / m_MovementSpeed;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;

                Vector3 currentPos = Vector3.Lerp(startPos, targetPos, progress);
                m_Transform.position = currentPos;

                // Animate movement using existing system
                Vector3 direction = (targetPos - startPos).normalized;
                Vector2 animAxis = new Vector2(
                    Vector3.Dot(direction, m_Transform.right),
                    Vector3.Dot(direction, m_Transform.forward)
                );
                m_Animation.Animate(animAxis, 0f, Time.deltaTime);

                // Face movement direction
                if (direction.magnitude > 0.1f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    m_Transform.rotation = Quaternion.RotateTowards(
                        m_Transform.rotation,
                        targetRotation,
                        m_RotateSpeed * Time.deltaTime
                    );
                }

                yield return null;
            }

            m_Transform.position = targetPos;
        }

        private IEnumerator LookAtTransform(Transform target, float duration)
        {
            float elapsed = 0f;
            Vector3 lookTarget = target.position;
            Vector3 originalTarget = m_Target; // Store original target
            LookWeight originalLookWeight = m_LookWeight; // Store original look weight

            Debug.Log($"Looking at {target.name} from {m_Transform.position} to {lookTarget}");
            Debug.Log($"Distance: {Vector3.Distance(m_Transform.position, lookTarget)}");

            // Check if target is valid
            if (Vector3.Distance(m_Transform.position, lookTarget) < 0.1f)
            {
                Debug.LogWarning($"Target {target.name} is too close or at same position");
                yield break;
            }

            // Set up head-only look behavior
            m_Target = lookTarget; // This feeds into the IK system
            LookWeight headLookWeight = new LookWeight(1f, 0.1f, 1f, 1f);

            // Smoothly transition to look behavior
            float blendDuration = 0.3f;
            elapsed = 0f;

            while (elapsed < blendDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / blendDuration;

                // Blend to head look
                m_LookWeight = LerpLookWeight(originalLookWeight, headLookWeight, progress);
                yield return null;
            }

            m_LookWeight = headLookWeight;

            // Hold the look for the specified duration
            yield return new WaitForSeconds(duration);

            // Smoothly return to original look behavior
            elapsed = 0f;
            while (elapsed < blendDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / blendDuration;

                m_LookWeight = LerpLookWeight(headLookWeight, originalLookWeight, progress);
                yield return null;
            }

            // Restore original values
            m_Target = originalTarget;
            m_LookWeight = originalLookWeight;
        }

        private LookWeight LerpLookWeight(LookWeight from, LookWeight to, float t)
        {
            return new LookWeight
            {
                weight = Mathf.Lerp(from.weight, to.weight, t),
                body = Mathf.Lerp(from.body, to.body, t),
                head = Mathf.Lerp(from.head, to.head, t),
                eyes = Mathf.Lerp(from.eyes, to.eyes, t),
            };
        }

        private IEnumerator FadeCoroutine(float fromAlpha, float toAlpha)
        {
            float elapsed = 0f;

            while (elapsed < m_FadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(fromAlpha, toAlpha, elapsed / m_FadeDuration);
                SetAlpha(alpha);
                yield return null;
            }

            SetAlpha(toAlpha);
        }

        private void SetAlpha(float alpha)
        {
            for (int i = 0; i < m_Renderers.Length; i++)
            {
                if (m_Renderers[i] != null && m_OriginalMaterials[i] != null)
                {
                    Material mat = m_Renderers[i].material;

                    // Try different transparency properties based on shader type
                    if (mat.HasProperty("_Color"))
                    {
                        Color color = m_OriginalMaterials[i].color;
                        color.a = alpha;
                        mat.color = color;
                    }

                    // For Standard shader transparency
                    if (mat.HasProperty("_Mode"))
                    {
                        mat.SetFloat("_Mode", 3); // Transparent mode
                        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        mat.SetInt(
                            "_DstBlend",
                            (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha
                        );
                        mat.SetInt("_ZWrite", 0);
                        mat.DisableKeyword("_ALPHATEST_ON");
                        mat.EnableKeyword("_ALPHABLEND_ON");
                        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        mat.renderQueue = 3000;
                    }

                    // For URP/Built-in renderer
                    if (mat.HasProperty("_BaseColor"))
                    {
                        Color baseColor = mat.GetColor("_BaseColor");
                        baseColor.a = alpha;
                        mat.SetColor("_BaseColor", baseColor);
                    }

                    // For legacy shaders
                    if (mat.HasProperty("_MainTex"))
                    {
                        Color mainColor = mat.GetColor("_Color");
                        mainColor.a = alpha;
                        mat.SetColor("_Color", mainColor);
                    }
                }
            }
        }

        [System.Serializable]
        public class TransformData
        {
            public Transform transform;
            public TransformType type = TransformType.Movement;
        }

        [System.Serializable]
        public class TransformSet
        {
            public string name;
            public List<TransformData> transforms = new List<TransformData>();
        }

        public enum TransformType
        {
            Movement,
            Look,
        }
    }
}
