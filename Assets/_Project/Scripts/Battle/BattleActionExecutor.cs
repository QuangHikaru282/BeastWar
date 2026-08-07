using System.Collections;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// Chuyên xử lý các hành động trong trận đấu: Tấn công, Tính sát thương, VFX, Bắt thú.
/// </summary>
public class BattleActionExecutor : MonoBehaviour
{
    public static BattleActionExecutor Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    /// <summary>
    /// Thực hiện chuỗi hành động tấn công (VFX, Animation, Trừ máu).
    /// </summary>
    public IEnumerator ExecuteAttack(BeastUnit attacker, BeastUnit target, RuntimeMoveData move)
    {
        if (attacker == null || !attacker.IsAlive) yield break;
        if (target == null   || !target.IsAlive)   yield break;

        string moveName = move != null ? move.baseMove.moveName : "Tấn công thường";
        MoveType type = move != null ? move.baseMove.moveType : MoveType.Melee;
        
        string battleLog = $"{attacker.Data.baseBeast.beastName} dùng {moveName}!";
        Debug.Log($"[Battle] {attacker.Data.baseBeast.beastName} dùng {moveName} ({type}) tấn công {target.Data.baseBeast.beastName}!");
        if (BattleUIManager.Instance != null && BattleUIManager.Instance.ActionPanel != null)
        {
            BattleUIManager.Instance.ActionPanel.SetGuide(battleLog);
        }

        Vector3 originalPos = attacker.transform.position;

        // Nếu có Animator xịn xò, ra lệnh chạy hoạt hình Attack
        Animator anim = attacker.GetComponent<Animator>();
        if (anim != null && anim.enabled)
        {
            anim.SetTrigger("Attack");
        }

        if (type == MoveType.Melee)
        {
            Vector3 dir = (target.transform.position - originalPos).normalized;
            Vector3 dashPos = target.transform.position - dir * 0.8f;
            yield return attacker.transform.DOMove(dashPos, 0.2f).SetEase(Ease.OutQuad).WaitForCompletion();
        }
        else if (type == MoveType.Ranged)
        {
            yield return attacker.transform.DOJump(originalPos, 0.5f, 1, 0.3f).WaitForCompletion();
        }
        else if (type == MoveType.Self)
        {
            yield return attacker.transform.DOJump(originalPos, 0.2f, 1, 0.25f).WaitForCompletion();
        }

        // Gọi VFX
        if (move != null && move.baseMove.vfxPrefab != null)
        {
            yield return StartCoroutine(HandleVFX(attacker, target, move));
        }

        // Xử lý logic chiêu thức
        if (type == MoveType.Self)
        {
            // CHIÊU BUFF / HỒI MÁU
            int healAmount = move != null ? move.power : 20; 
            attacker.Heal(healAmount);
            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            // TẤN CÔNG
            string effectivenessMsg = "";
            int baseDamage = move != null ? attacker.CalculateDamage(target, move, out effectivenessMsg) : attacker.CalculateBaseDamage(target);
            bool isCrit = false;
            int finalDamage = baseDamage;

            if (attacker.IsPlayerTeam)
            {
                isCrit = UnityEngine.Random.value < attacker.CritChance;
                if (isCrit) finalDamage = Mathf.RoundToInt(baseDamage * attacker.CritMultiplier);
            }

            bool died = target.TakeDamageWithResult(finalDamage, isCrit);

            // Nếu đòn đánh có hiệu ứng khắc hệ -> Hiện chữ lên DialogueText
            if (!string.IsNullOrEmpty(effectivenessMsg))
            {
                if (BattleUIManager.Instance != null && BattleUIManager.Instance.ActionPanel != null)
                {
                    BattleUIManager.Instance.ActionPanel.SetGuide(effectivenessMsg);
                }
                yield return new WaitForSeconds(0.8f);
            }

            // Xử lý áp dụng Hiệu ứng Bất lợi (Status Effect) từ MoveData theo tỉ lệ %
            if (!died && target.IsAlive && move != null && move.baseMove != null && move.baseMove.statusToApply != BeastUnit.StatusEffect.None)
            {
                float rand = UnityEngine.Random.Range(0f, 100f);
                if (rand <= move.baseMove.statusChance)
                {
                    target.ApplyStatus(move.baseMove.statusToApply);
                    Debug.Log($"[Status] {attacker.Data.baseBeast.beastName} đã gây hiệu ứng {move.baseMove.statusToApply} lên {target.Data.baseBeast.beastName} ({move.baseMove.statusChance}%)!");
                }
            }

            Debug.Log($"[Battle] {attacker.Data.baseBeast.beastName} gây {finalDamage} sát thương{(isCrit ? " (CRIT!" + ")": "")}! {target.Data.baseBeast.beastName} HP: {target.CurrentHP}");

            yield return new WaitForSeconds(0.2f);



            if (type == MoveType.Melee)
            {
                yield return attacker.transform.DOMove(originalPos, 0.25f).SetEase(Ease.InQuad).WaitForCompletion();
            }

            if (died)
            {
                Debug.Log($"[Battle] {target.Data.baseBeast.beastName} đã chết!");
                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    private IEnumerator HandleVFX(BeastUnit attacker, BeastUnit target, RuntimeMoveData move)
    {
        if (move.baseMove.vfxSpawnType == VfxSpawnType.SpawnAtTarget)
        {
            GameObject vfx = Instantiate(move.baseMove.vfxPrefab, target.transform.position, move.baseMove.vfxPrefab.transform.rotation);
            Destroy(vfx, 1.5f); 
        }
        else if (move.baseMove.vfxSpawnType == VfxSpawnType.ShootFromAttacker)
        {
            GameObject projectile = Instantiate(move.baseMove.vfxPrefab, attacker.transform.position, Quaternion.identity);
            
            Vector3 dirToTarget = (target.transform.position - attacker.transform.position).normalized;
            float angle = Mathf.Atan2(dirToTarget.y, dirToTarget.x) * Mathf.Rad2Deg;
            projectile.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            projectile.transform.DOMove(target.transform.position, 0.3f).SetEase(Ease.Linear).OnComplete(() => {
                Animator anim = projectile.GetComponentInChildren<Animator>();
                if (anim != null) anim.Play("Hit");
                Destroy(projectile, 0.5f);
            });

            yield return new WaitForSeconds(0.3f);
        }
        else if (move.baseMove.vfxSpawnType == VfxSpawnType.RainFromSky)
        {
            Vector3 skyPos = target.transform.position + Vector3.up * 5f;
            GameObject projectile = Instantiate(move.baseMove.vfxPrefab, skyPos, move.baseMove.vfxPrefab.transform.rotation);
            
            projectile.transform.DOMove(target.transform.position, 0.4f).SetEase(Ease.InQuad).OnComplete(() => {
                Animator anim = projectile.GetComponentInChildren<Animator>();
                if (anim != null) anim.Play("Hit");
                Destroy(projectile, 0.5f);
            });

            yield return new WaitForSeconds(0.4f);
        }
        else if (move.baseMove.vfxSpawnType == VfxSpawnType.SpawnAtSelf)
        {
            GameObject vfx = Instantiate(move.baseMove.vfxPrefab, attacker.transform.position, move.baseMove.vfxPrefab.transform.rotation);
            Destroy(vfx, 1.5f);
        }
    }
}
