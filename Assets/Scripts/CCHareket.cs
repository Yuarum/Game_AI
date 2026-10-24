using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class CCHareket : MonoBehaviour
{
    [SerializeField] private Transform kamera;
    [SerializeField] private float yurumeHizi = 4f;
    [SerializeField] private float kosmaHizi = 7f;
    [SerializeField] private float ziplamaYuksekligi = 1.5f;
    [SerializeField] private float yercekimi = -9.81f;
    [SerializeField] private float donusHizi = 720f;

    private CharacterController cc;
    private InputAction move, jump, sprint;
    private float dikeyHiz;

    private void Awake()
    {
        cc     = GetComponent<CharacterController>();
        move   = InputSystem.actions.FindAction("Player/Move");
        jump   = InputSystem.actions.FindAction("Player/Jump");
        sprint = InputSystem.actions.FindAction("Player/Sprint");
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        Vector3 yon = Hareket.IstenenYon(move.ReadValue<Vector2>(), kamera);
        float hiz = sprint.IsPressed() ? kosmaHizi : yurumeHizi;

        if (cc.isGrounded && dikeyHiz < 0f) dikeyHiz = -2f;
        if (cc.isGrounded && jump.WasPressedThisFrame())
            dikeyHiz = Mathf.Sqrt(2f * -yercekimi * ziplamaYuksekligi);

        dikeyHiz += yercekimi * dt;
        Vector3 hizVektoru = yon * hiz + Vector3.up * dikeyHiz;
        cc.Move(hizVektoru * dt);

        if (yon.sqrMagnitude > 0.001f)
        {
            Quaternion hedef = Quaternion.LookRotation(yon);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, hedef, donusHizi * dt);
        }
    }
}