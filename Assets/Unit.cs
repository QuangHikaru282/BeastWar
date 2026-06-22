using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Move
{
    public string moveName;        // Tên chiêu
    public int damage;             // Sát thương của chiêu
    public string animTrigger;     // Tên Trigger Animation (vd: "Attack")
}

public class Unit : MonoBehaviour
{

	public string unitName;
	public int unitLevel;

	public Move[] moves; // Danh sách các chiêu thức của Pet

	public int maxHP;
	public int currentHP;

	public void Setup(BeastData data, bool isPlayer)
	{
		unitName = data.beastName;
		unitLevel = 1; // Mặc định level 1 vì BeastData chưa có Level
		maxHP = data.maxHP;
		currentHP = data.maxHP; // Hồi đầy máu

		// Cập nhật hình ảnh (Quái của mình thì hiện hình từ đằng sau, địch thì hiện hình đằng trước)
		SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
		if (sr != null)
		{
			sr.sprite = isPlayer ? data.backSprite : data.frontSprite;
		}

		// Cập nhật animation
		Animator anim = GetComponentInChildren<Animator>();
		if (anim != null && data.animatorController != null)
		{
			anim.runtimeAnimatorController = data.animatorController;
		}

		// Cập nhật chiêu thức
		if (data.moves != null && data.moves.Length > 0)
		{
			moves = new Move[data.moves.Length];
			for (int i = 0; i < data.moves.Length; i++)
			{
				moves[i] = new Move();
				moves[i].moveName = data.moves[i].moveName;
				moves[i].damage = data.moves[i].power; // Ánh xạ power sang damage
				moves[i].animTrigger = "Attack"; // Mặc định là Attack
			}
		}
	}

	public bool TakeDamage(int dmg)
	{
		currentHP -= dmg;

		if (currentHP <= 0)
			return true;
		else
			return false;
	}

	public void Heal(int amount)
	{
		currentHP += amount;
		if (currentHP > maxHP)
			currentHP = maxHP;
	}

}
