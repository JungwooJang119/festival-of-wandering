using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{

    private PlayerInput _input;
    private Rigidbody2D _rb;
    private Animator _an;
    private PlayerAttackComponent _attackComponent;
    private WeaponEmitter _weaponEmitter;
    [SerializeField] private CinemachineCameraOffset _cco;

    [SerializeField] private PlayerWeaponBase[] weapons = new PlayerWeaponBase[2];
    [SerializeField] private Weapon spell;

    private Vector2 moveDir;
    private Vector2 aimDir;

    [SerializeField] private float baseSpeed = 3f;
    [SerializeField] private float maxAimOffset = 2;

    private bool dodging = false;
    [SerializeField] private float dodgeSpeedFactor = 1.75f;
    [SerializeField] private float dodgeTime = 1;

    // Start is called before the first frame update
    void Start()
    {
        _input = GetComponent<PlayerInput>();
        _rb = GetComponent<Rigidbody2D>();
        _an = GetComponent<Animator>();
        _attackComponent = GetComponentInChildren<PlayerAttackComponent>();
        _weaponEmitter = GetComponentInChildren<WeaponEmitter>();
    }

    // Update is called once per frame
    void Update()
    {
        if (dodging) return;
        moveDir = _input.currentActionMap["Move"].ReadValue<Vector2>();
        aimDir = _input.currentActionMap["Gamepad Aim"].ReadValue<Vector2>();
        // Debug.Log(aimDir);
        if (_cco) {
            _cco.m_Offset = Vector2.ClampMagnitude(aimDir, 1) * maxAimOffset;
        }
        if (aimDir.magnitude == 0) aimDir = moveDir;
        Vector2 normalizedAim = aimDir.normalized;
        _an.SetFloat("facingX", normalizedAim.x);
        _an.SetFloat("facingY", normalizedAim.y);
    }

    private void FixedUpdate()
    {
        float speed = baseSpeed;
        if (_attackComponent.Attacking) speed *= _attackComponent.CurrAttack.MovementSpeed;
        _rb.velocity = Vector2.ClampMagnitude(moveDir, 1) * speed;
    }

    void OnPrimaryAttack() {
        if (weapons[0])
            _attackComponent.TriggerWeapon(weapons[0], aimDir);
    }

    void OnSecondaryAttack() {
        if (weapons[1])
            _attackComponent.TriggerWeapon(weapons[1], aimDir);
    }

    void OnSpell() {
        if (!spell || _weaponEmitter.FiringActive) return;
        _weaponEmitter.Fire(spell, aimDir);
    }

    IEnumerator Dodge(Vector2 dir) {
        dodging = true;
        float dodgeSpeed = dodgeSpeedFactor * baseSpeed;
        float elapsedTime = 0;
        while (elapsedTime < dodgeTime)
        {
            Vector3 displacement = dodgeSpeed * dir * Time.deltaTime;
            transform.position += displacement;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        dodging = false;
    }

    void OnDodge() {
        if (!dodging) {
            Vector2 dodgeDir = moveDir;
            if (moveDir == Vector2.zero) dodgeDir = aimDir;
            StartCoroutine(Dodge(dodgeDir.normalized));
        }
    }
}
