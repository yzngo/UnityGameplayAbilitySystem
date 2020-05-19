using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHandler : MonoBehaviour {
    public enum Action { Equip, Unequip };

    public Transform Weapon;
    public Transform WeaponHandle;
    public Transform WeaponRestPose;

    public bool IsWeaponEquipped;
    public void ResetWeapon(Action action) {
        if (action == Action.Equip) {
            Weapon.SetParent(WeaponHandle);
        } else {
            Weapon.SetParent(WeaponRestPose);
        }
        Weapon.localRotation = Quaternion.identity;
        Weapon.localPosition = Vector3.zero;

    }
}
