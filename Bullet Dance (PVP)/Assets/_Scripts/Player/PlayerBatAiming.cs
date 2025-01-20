using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBatAiming : NetworkBehaviour
{
    [Header("Stats")]
    [SerializeField] private float deflectionDuration;
    [SerializeField] private float deflectionCooldown;
    [SerializeField] private float attackRange;
    [SerializeField] private float attackWidth;

    [Header("Emote")]
    [SerializeField] private GameObject emote1;
    [SerializeField] private GameObject emote2;

    [Header("References")]
    [SerializeField] private LayerMask deflectableLayer;
    [SerializeField] private GameObject deflectionPoint;
    [SerializeField] private InputActionReference mousePosition;

    private InputActions actions;
    private Vector2 mouseInputPosition;
    private Vector2 playerFacingDirection;
    private BatParent batParent;
    bool isSwinging;
    List<RaycastHit2D> hits;


    private void Awake()
    {
        batParent = GetComponentInChildren<BatParent>();

        actions = new InputActions();
        actions.Player.Deflect.started += ctx => DeflectionServerRpc(playerFacingDirection);
        actions.Player.Emote1.started += ctx => EmoteClientRpc(true);
        actions.Player.Emote2.started += ctx => EmoteClientRpc(false);
    }

    private void Update()
    {
        mouseInputPosition = GetMouseInputPosition();
        batParent.MousePosition = mouseInputPosition;

        playerFacingDirection = mouseInputPosition - (Vector2)transform.position;
    }


    private Vector2 GetMouseInputPosition()
    {
        Vector3 mousePos = mousePosition.action.ReadValue<Vector2>();
        mousePos.z = Camera.main.nearClipPlane;
        return Camera.main.ScreenToWorldPoint(mousePos);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void EmoteClientRpc(bool isEmote1)
    {
        StartCoroutine(EmoteRoutine(isEmote1));
    }
    private IEnumerator EmoteRoutine(bool isEmote1)
    {
        if (isEmote1) emote1.SetActive(true);
        else emote2.SetActive(true);

        yield return new WaitForSeconds(1);

        if (isEmote1) emote1.SetActive(false);
        else emote2.SetActive(false);

        yield break;
    }
    [Rpc(SendTo.Server)]
    private void DeflectionServerRpc(Vector2 deflectDir)
    {
        if (!isSwinging) StartCoroutine(DeflectionRoutine(deflectDir));
    }
    IEnumerator DeflectionRoutine(Vector2 deflectDir)
    {
        hits = Physics2D.BoxCastAll(deflectionPoint.transform.position, new Vector2(attackWidth, attackRange), 0, deflectDir, 0, deflectableLayer).ToList();

        for (int i = 0; i < hits.Count; i++)
        {
            Bullet bullet = hits[i].collider.gameObject.GetComponent<Bullet>();
            if (bullet != null) bullet.Deflect(deflectDir);
        }

        deflectionPoint.SetActive(true);
        isSwinging = true;


        yield return new WaitForSeconds(deflectionDuration);
        
        deflectionPoint.SetActive(false);
        
        
        yield return new WaitForSeconds(deflectionCooldown);

        isSwinging = false;

        yield break;
    }


    private void OnEnable()
    {
        actions.Enable();
    }

    private void OnDisable()
    {
        actions.Disable();
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawCube(deflectionPoint.transform.position, new Vector3(attackWidth, attackRange, 0));
    }
}
