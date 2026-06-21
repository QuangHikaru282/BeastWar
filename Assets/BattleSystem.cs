using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum BattleState { START, PLAYERTURN, ENEMYTURN, WON, LOST }

public class BattleSystem : MonoBehaviour
{
	[Header("Game Data")]
	public PlayerData playerData;
	public BattleTransferData battleTransferData;

	[Header("Base Prefab")]
	public GameObject baseUnitPrefab;

	private int currentPlayerIndex = 0;
	private int currentEnemyIndex = 0;

	public Transform playerBattleStation;
	public Transform enemyBattleStation;

	public Button[] moveButtons;
	public Text[] moveButtonTexts;

	Unit playerUnit;
	Unit enemyUnit;

	public Text dialogueText;

	public BattleHUD playerHUD;
	public BattleHUD enemyHUD;

	public BattleState state;

    // Start is called before the first frame update
    void Start()
    {
		state = BattleState.START;
		StartCoroutine(SetupBattle());
    }

	IEnumerator SetupBattle()
	{
		currentPlayerIndex = 0;
		currentEnemyIndex = 0;

		SpawnPlayerUnit();
		SpawnEnemyUnit();

		dialogueText.text = "A wild " + enemyUnit.unitName + " approaches...";

		yield return new WaitForSeconds(2f);

		state = BattleState.PLAYERTURN;
		PlayerTurn();
	}

	void SpawnPlayerUnit()
	{
		if (playerUnit != null) Destroy(playerUnit.gameObject);
		
		BeastData data = null;
		if (playerData != null && playerData.currentFormation.Count > 0)
		{
			if (currentPlayerIndex < playerData.currentFormation.Count)
				data = playerData.currentFormation[currentPlayerIndex];
		}

		GameObject playerGO = Instantiate(baseUnitPrefab, playerBattleStation);
		playerUnit = playerGO.GetComponent<Unit>();
		
		if (data != null) playerUnit.Setup(data, true);
		
		playerHUD.SetHUD(playerUnit);
	}

	void SpawnEnemyUnit()
	{
		if (enemyUnit != null) Destroy(enemyUnit.gameObject);

		BeastData data = null;
		if (battleTransferData != null && battleTransferData.wildEnemyTeam.Count > 0)
		{
			if (currentEnemyIndex < battleTransferData.wildEnemyTeam.Count)
				data = battleTransferData.wildEnemyTeam[currentEnemyIndex];
		}

		GameObject enemyGO = Instantiate(baseUnitPrefab, enemyBattleStation);
		enemyUnit = enemyGO.GetComponent<Unit>();
		
		if (data != null) enemyUnit.Setup(data, false);

		enemyHUD.SetHUD(enemyUnit);
	}

	IEnumerator PlayerAttack(Move usedMove)
	{
		// Ẩn các nút chiêu thức khi đang đánh
		for (int i = 0; i < moveButtons.Length; i++) moveButtons[i].gameObject.SetActive(false);

		dialogueText.text = playerUnit.unitName + " uses " + usedMove.moveName + "!";
		yield return new WaitForSeconds(1f);

		// Kích hoạt Animation tấn công theo chiêu thức
		Animator anim = playerUnit.GetComponent<Animator>();
		if (anim != null && !string.IsNullOrEmpty(usedMove.animTrigger)) 
			anim.SetTrigger(usedMove.animTrigger);

		// Đợi 0.5 giây để khớp với lúc vung đòn trúng đích
		yield return new WaitForSeconds(0.5f);

		bool isDead = enemyUnit.TakeDamage(usedMove.damage);

		enemyHUD.SetHP(enemyUnit.currentHP);
		dialogueText.text = "The attack is successful!";

		yield return new WaitForSeconds(2f);

		if(isDead)
		{
			currentEnemyIndex++;
			int maxEnemies = (battleTransferData != null) ? battleTransferData.wildEnemyTeam.Count : 1;
			if (currentEnemyIndex < maxEnemies)
			{
				dialogueText.text = enemyUnit.unitName + " fainted!";
				yield return new WaitForSeconds(2f);
				SpawnEnemyUnit();
				dialogueText.text = "Enemy sends out " + enemyUnit.unitName + "!";
				yield return new WaitForSeconds(2f);
				
				state = BattleState.PLAYERTURN;
				PlayerTurn();
			}
			else
			{
				state = BattleState.WON;
				EndBattle();
			}
		} else
		{
			state = BattleState.ENEMYTURN;
			StartCoroutine(EnemyTurn());
		}
	}

	IEnumerator EnemyTurn()
	{
		// Random chọn 1 chiêu thức của Enemy
		Move usedMove = enemyUnit.moves[Random.Range(0, enemyUnit.moves.Length)];

		dialogueText.text = enemyUnit.unitName + " uses " + usedMove.moveName + "!";
		yield return new WaitForSeconds(1f);

		// Kích hoạt Animation tấn công của Enemy
		Animator anim = enemyUnit.GetComponent<Animator>();
		if (anim != null && !string.IsNullOrEmpty(usedMove.animTrigger)) 
			anim.SetTrigger(usedMove.animTrigger);

		yield return new WaitForSeconds(0.5f);

		bool isDead = playerUnit.TakeDamage(usedMove.damage);

		playerHUD.SetHP(playerUnit.currentHP);

		yield return new WaitForSeconds(1f);

		if(isDead)
		{
			currentPlayerIndex++;
			int maxPlayers = (playerData != null) ? playerData.currentFormation.Count : 1;
			if (currentPlayerIndex < maxPlayers)
			{
				dialogueText.text = playerUnit.unitName + " fainted!";
				yield return new WaitForSeconds(2f);
				SpawnPlayerUnit();
				dialogueText.text = "Go " + playerUnit.unitName + "!";
				yield return new WaitForSeconds(2f);

				state = BattleState.PLAYERTURN;
				PlayerTurn();
			}
			else
			{
				state = BattleState.LOST;
				EndBattle();
			}
		} else
		{
			state = BattleState.PLAYERTURN;
			PlayerTurn();
		}

	}

	void EndBattle()
	{
		StartCoroutine(EndBattleCoroutine());
	}

	IEnumerator EndBattleCoroutine()
	{
		if(state == BattleState.WON)
		{
			dialogueText.text = "You won the battle!";
			// Làm choáng quái sau khi thắng
			if (battleTransferData != null && !string.IsNullOrEmpty(battleTransferData.lastEncounteredBeastId))
			{
				if (!battleTransferData.stunnedBeastIds.Contains(battleTransferData.lastEncounteredBeastId))
				{
					battleTransferData.stunnedBeastIds.Add(battleTransferData.lastEncounteredBeastId);
				}
			}
		} else if (state == BattleState.LOST)
		{
			dialogueText.text = "You were defeated.";
		}

		yield return new WaitForSeconds(2f);

		// Trở về map trước đó
		if (battleTransferData != null)
		{
			if (battleTransferData.originScene == BattleTransferData.OriginScene.Map)
				UnityEngine.SceneManagement.SceneManager.LoadScene(GameSceneManager.SCENE_MAP);
			else if (battleTransferData.originScene == BattleTransferData.OriginScene.Hunting)
				UnityEngine.SceneManagement.SceneManager.LoadScene(GameSceneManager.SCENE_HUNTING);
			else if (battleTransferData.originScene == BattleTransferData.OriginScene.WorldMap)
				UnityEngine.SceneManagement.SceneManager.LoadScene(GameSceneManager.SCENE_WORLDMAP);
			else
				UnityEngine.SceneManagement.SceneManager.LoadScene(GameSceneManager.SCENE_MAP);
		}
		else
		{
			UnityEngine.SceneManagement.SceneManager.LoadScene(GameSceneManager.SCENE_MAP);
		}
	}

	void PlayerTurn()
	{
		dialogueText.text = "Choose an action:";
		
		// Bật các nút chiêu thức dựa vào số chiêu của Pet
		for (int i = 0; i < moveButtons.Length; i++)
		{
			if (i < playerUnit.moves.Length)
			{
				moveButtons[i].gameObject.SetActive(true);
				moveButtonTexts[i].text = playerUnit.moves[i].moveName;
			}
			else
			{
				moveButtons[i].gameObject.SetActive(false);
			}
		}
	}

	IEnumerator PlayerHeal()
	{
		// Ẩn các nút chiêu thức khi đang hồi máu
		for (int i = 0; i < moveButtons.Length; i++) moveButtons[i].gameObject.SetActive(false);

		playerUnit.Heal(5);

		playerHUD.SetHP(playerUnit.currentHP);
		dialogueText.text = "You feel renewed strength!";

		yield return new WaitForSeconds(2f);

		state = BattleState.ENEMYTURN;
		StartCoroutine(EnemyTurn());
	}

	public void OnMoveButton(int moveIndex)
	{
		if (state != BattleState.PLAYERTURN)
			return;

		if (moveIndex < playerUnit.moves.Length)
		{
			StartCoroutine(PlayerAttack(playerUnit.moves[moveIndex]));
		}
	}

	public void OnHealButton()
	{
		if (state != BattleState.PLAYERTURN)
			return;

		StartCoroutine(PlayerHeal());
	}

}
