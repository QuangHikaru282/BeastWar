using UnityEngine;

public class PlayerAvatarSelector : MonoBehaviour
{
    [Header("Dữ liệu")]
    public PlayerData playerData;

    [Header("Mô hình nhân vật")]
    [Tooltip("Sub-GameObject chứa sprite/animator của Nam")]
    public GameObject maleModel;

    [Tooltip("Sub-GameObject chứa sprite/animator của Nữ")]
    public GameObject femaleModel;

    private void Awake()
    {
        UpdateAvatar();
    }

    public void UpdateAvatar()
    {
        if (playerData == null)
        {
            Debug.LogWarning("[PlayerAvatarSelector] Chưa gán PlayerData!");
            return;
        }

        if (maleModel == null || femaleModel == null)
        {
            Debug.LogWarning("[PlayerAvatarSelector] Chưa gán đủ mô hình Nam/Nữ!");
            return;
        }

        bool isFemale = (playerData.characterGender == "Female");

        maleModel.SetActive(!isFemale);
        femaleModel.SetActive(isFemale);

        GameObject activeModel = isFemale ? femaleModel : maleModel;
        Animator anim = activeModel.GetComponent<Animator>();

        // Cập nhật cho PlayerMapController
        PlayerMapController mapController = GetComponent<PlayerMapController>();
        if (mapController != null)
        {
            mapController.SetAnimator(anim);
        }

        // Cập nhật cho TopDownCharacterController (Cainos) nếu có sử dụng
        MonoBehaviour cainosController = GetComponent("TopDownCharacterController") as MonoBehaviour;
        if (cainosController != null)
        {
            var field = cainosController.GetType().GetField("animator");
            if (field != null)
            {
                field.SetValue(cainosController, anim);
            }
        }

        // Cập nhật cho Kinnly.PlayerMovement
        Kinnly.PlayerMovement kinnlyMovement = GetComponent<Kinnly.PlayerMovement>();
        if (kinnlyMovement != null)
        {
            kinnlyMovement.animator = anim;
        }

        Debug.Log($"[PlayerAvatarSelector] Đã thiết lập ngoại hình nhân vật: {(isFemale ? "Nữ" : "Nam")}");
    }
}
