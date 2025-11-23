using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Helper script to quickly set up a kidnapper bot with all required components.
/// Attach this to an empty GameObject, configure it, then click "Setup Kidnapper" button in Inspector.
/// </summary>
[System.Serializable]
public class KidnapperSetupHelper : MonoBehaviour
{
    [Header("Character Model")]
    [Tooltip("Drag the NPC character model prefab here")]
    public GameObject characterModel;

    [Header("Animator Controller")]
    [Tooltip("Drag the Animator Controller here")]
    public RuntimeAnimatorController animatorController;

    [Header("Patrol Points")]
    [Tooltip("Create empty GameObjects for patrol points and drag them here")]
    public Transform[] patrolPoints;

    [Header("AI Settings")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public float detectionRange = 10f;
    public float attackRange = 2f;
    public float fieldOfView = 90f;

    [ContextMenu("Setup Kidnapper")]
    public void SetupKidnapper()
    {
        // Add NavMesh Agent if not present
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            agent = gameObject.AddComponent<NavMeshAgent>();
            Debug.Log("Added NavMeshAgent component");
        }

        // Configure NavMesh Agent
        agent.speed = patrolSpeed;
        agent.height = 2f;
        agent.radius = 0.5f;
        agent.stoppingDistance = attackRange;

        // Add Capsule Collider if not present
        CapsuleCollider collider = GetComponent<CapsuleCollider>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<CapsuleCollider>();
            collider.height = 2f;
            collider.radius = 0.5f;
            collider.center = new Vector3(0, 1, 0);
            Debug.Log("Added CapsuleCollider component");
        }

        // Add KidnapperAI script if not present
        KidnapperAI ai = GetComponent<KidnapperAI>();
        if (ai == null)
        {
            ai = gameObject.AddComponent<KidnapperAI>();
            Debug.Log("Added KidnapperAI component");
        }

        // Configure KidnapperAI
        ai.patrolSpeed = patrolSpeed;
        ai.chaseSpeed = chaseSpeed;
        ai.detectionRange = detectionRange;
        ai.attackRange = attackRange;
        ai.fieldOfView = fieldOfView;
        ai.patrolPoints = patrolPoints;

        // Set up character model
        if (characterModel != null)
        {
            // Check if model is already a child
            bool isChild = false;
            foreach (Transform child in transform)
            {
                if (child.gameObject == characterModel)
                {
                    isChild = true;
                    break;
                }
            }

            if (!isChild)
            {
                // Instantiate model as child
                GameObject modelInstance = Instantiate(characterModel, transform);
                modelInstance.transform.localPosition = Vector3.zero;
                modelInstance.transform.localRotation = Quaternion.identity;
                characterModel = modelInstance;
                Debug.Log("Added character model as child");
            }

            // Set up Animator
            Animator animator = characterModel.GetComponent<Animator>();
            if (animator == null)
            {
                animator = characterModel.AddComponent<Animator>();
                Debug.Log("Added Animator component to character model");
            }

            if (animatorController != null)
            {
                animator.runtimeAnimatorController = animatorController;
                Debug.Log("Assigned Animator Controller");
            }

            // Assign animator to AI script
            ai.animator = animator;
        }

        // Set layer to Enemy if it exists
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer != -1)
        {
            gameObject.layer = enemyLayer;
        }

        Debug.Log("Kidnapper setup complete! Don't forget to:");
        Debug.Log("1. Set up NavMesh (Window > AI > Navigation > Bake)");
        Debug.Log("2. Verify Player has 'Player' tag");
        Debug.Log("3. Test in Play Mode");
    }

    [ContextMenu("Create Patrol Points")]
    public void CreatePatrolPoints()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            // Create 3 default patrol points
            Transform[] newPoints = new Transform[3];
            for (int i = 0; i < 3; i++)
            {
                GameObject point = new GameObject($"PatrolPoint_{i + 1}");
                point.transform.position = transform.position + new Vector3(
                    Mathf.Cos(i * 120f * Mathf.Deg2Rad) * 5f,
                    0,
                    Mathf.Sin(i * 120f * Mathf.Deg2Rad) * 5f
                );
                point.transform.parent = transform.parent; // Keep in scene, not as child
                newPoints[i] = point.transform;
            }
            patrolPoints = newPoints;
            Debug.Log("Created 3 default patrol points around the kidnapper");
        }
    }
}


