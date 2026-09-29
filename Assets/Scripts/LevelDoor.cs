using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class LevelDoor : MonoBehaviour
{
    [Header("Door Type")]
    [SerializeField] private bool canInteract = true;
    [SerializeField] private bool playArrivalSequence = false;

    [Header("Scene Transition")]
    [SerializeField] private string targetSceneName;

    [Header("References")]
    [SerializeField] private Transform playerPosition;
    [SerializeField] private Animator doorAnimator;

    [Header("Animator State Names")]
    [SerializeField] private string openingStateName = "Opening";
    [SerializeField] private string closingStateName = "Closing";
    [SerializeField] private string doorInStateName = "Door In";
    [SerializeField] private string doorOutStateName = "Door Out";

    private bool isTransitioning;

    private void Awake()
    {
        if (doorAnimator == null)
        {
            doorAnimator = GetComponent<Animator>();
        }
    }

    private void Start()
    {
        if (playArrivalSequence)
        {
            StartCoroutine(ArrivalSequence());
        }
    }

    public void TryEnter(GameObject player)
    {
        if (!canInteract || isTransitioning)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(targetSceneName))
        {
            Debug.LogWarning(
                $"Door {gameObject.name} has no target scene.",
                this
            );

            return;
        }

        StartCoroutine(EnterDoorSequence(player));
    }

    private IEnumerator EnterDoorSequence(GameObject player)
    {
        isTransitioning = true;

        LockPlayer(player);
        MovePlayerToDoorPosition(player);

        yield return PlayAnimationAndWait(
            doorAnimator,
            "Open",
            openingStateName
        );

        Animator playerAnimator = player.GetComponent<Animator>();

        yield return PlayAnimationAndWait(
            playerAnimator,
            "DoorIn",
            doorInStateName
        );

        SceneManager.LoadScene(targetSceneName);
    }

private IEnumerator ArrivalSequence()
    {
        isTransitioning = true;

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogError(
                $"Cannot find Player in scene {SceneManager.GetActiveScene().name}.",
                this
            );

            isTransitioning = false;
            yield break;
        }

        Animator playerAnimator = player.GetComponent<Animator>();
        SpriteRenderer playerRenderer = player.GetComponent<SpriteRenderer>();

        LockPlayer(player);
        MovePlayerToDoorPosition(player);

        if (playerRenderer != null)
        {
            playerRenderer.enabled = false;
        }

        yield return PlayAnimationAndWait(
            doorAnimator,
            "Open",
            openingStateName
        );

        if (playerRenderer != null)
        {
            playerRenderer.enabled = true;
        }

        yield return PlayAnimationAndWait(
            playerAnimator,
            "DoorOut",
            doorOutStateName
        );

        yield return PlayAnimationAndWait(
            doorAnimator,
            "Close",
            closingStateName
        );

        UnlockPlayer(player);

        isTransitioning = false;
    }

    private IEnumerator PlayAnimationAndWait(
        Animator targetAnimator,
        string triggerName,
        string stateName
    )
    {
        if (targetAnimator == null)
        {
            Debug.LogError(
                $"Missing Animator when trying to play {stateName}.",
                this
            );

            yield break;
        }

        targetAnimator.SetTrigger(triggerName);

        yield return null;

        yield return new WaitUntil(() =>
            targetAnimator.GetCurrentAnimatorStateInfo(0).IsName(stateName)
        );

        yield return new WaitUntil(() =>
            targetAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f
        );
    }

    private void LockPlayer(GameObject player)
    {
        PlayerInput playerInput = player.GetComponent<PlayerInput>();
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        Rigidbody2D playerRigidbody = player.GetComponent<Rigidbody2D>();

        playerInput?.DeactivateInput();

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector2.zero;
        }
    }

    private void UnlockPlayer(GameObject player)
    {
        PlayerInput playerInput = player.GetComponent<PlayerInput>();
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();

        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        playerInput?.ActivateInput();
    }

    private void MovePlayerToDoorPosition(GameObject player)
    {
        if (playerPosition == null)
        {
            Debug.LogWarning(
                $"Door {gameObject.name} has no Player Position.",
                this
            );

            return;
        }

        player.transform.position = new Vector3(
            playerPosition.position.x,
            playerPosition.position.y,
            player.transform.position.z
        );
    }
}