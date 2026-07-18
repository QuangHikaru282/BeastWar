using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class FishingMinigame : MonoBehaviour
{
	/// <summary>
	/// This is the main fishing minigame code. It controls the states of fishing,
	/// then when in the minigame state it controls the movement of the catching
	/// bar and selects which fish we are catching.
	/// </summary>
	
	//These bools are just for keeping track of the state of the minigame we are in
	private bool lineCast = false;
    private bool nibble = false;
    public bool reelingFish = false;

    private Fish currentFishOnLine; //Reference to the current fish we are catching (Fish class is in Fish.cs)

    //These are references for the gameobjects used in the UI
    [Header("Setup References")]
    //The catching bar is the green bar that you put ontop of the fish to catch it
    [SerializeField] private GameObject catchingbar;
    private Vector3 catchingBarLoc;
    private Rigidbody2D catchingBarRB;
    
    //This is the fish on the UI that you are chasing to catch
    [SerializeField] private GameObject fishBar;
    private FishingMinigame_FishTrigger fishTrigger; //Reference to this script on the fish
    private bool inTrigger = false; //Whether or not the fish is inside the "catchingbar"
    
    private float catchPercentage = 0f; //0-100 how much you have caught the fish
    [SerializeField] private Slider catchProgressBar; //The bar on the right that shows how much you have caught

    [SerializeField] private GameObject thoughtBubbles;
    [SerializeField] private GameObject minigameCanvas;

    [Header("Settings")]
    [SerializeField] private KeyCode fishingKey = KeyCode.Space; //Key used to play
    [SerializeField] private float catchMultiplier = 10f; //Higher means catch fish faster x
    [SerializeField] private float catchingForce; //How much force to push the catchingbar up by
    
    [Header("Wild Beast Encounters")]
    [Tooltip("Tỷ lệ % câu lên một con Thú (0 - 100)")]
    [SerializeField] private float beastEncounterChance = 15f; 
    [Tooltip("Danh sách các Thú hệ nước có thể câu được")]
    [SerializeField] private System.Collections.Generic.List<BeastData> possibleWaterBeasts;
    [Tooltip("Dữ liệu truyền sang BattleScene")]
    [SerializeField] private BattleTransferData battleTransferData;
    
    private void Start() {
	    catchingBarRB = catchingbar.GetComponent<Rigidbody2D>(); //Get reference to the Rigidbody on the catchingbar
	    catchingBarLoc = catchingbar.GetComponent<RectTransform>().localPosition; //Use this to reset the catchingbars position to the bottom of the "water"
    }

    private void Update() {
	    if (!reelingFish) { //If we arent currently in the reeling stage
		    if (Input.GetKeyDown(fishingKey) && !lineCast) { //This is if we are doing nothing and are ready to cast a line
			    CastLine();
		    }else if (Input.GetKeyDown(fishingKey) && lineCast && !nibble) { //This is if the line has cast and we reel in before we get a nibble
			    StopAllCoroutines(); //Stops the WaitForNibble timer
			    lineCast = false; //Resets the line being cast
			    
			    //Resets the thought bubbles
			    thoughtBubbles.GetComponent<Animator>().SetTrigger("Reset");
			    thoughtBubbles.SetActive(false);
			    
		    }else if (Input.GetKeyDown(fishingKey) && lineCast && nibble) { //This is if we reel in while there is a nibble
			    StopAllCoroutines(); //Stops the LineBreak timer
			    StartReeling();
		    }
	    } else { //This is when we are in the stage where we are fighitng for the fish
		    if (Input.GetKey(fishingKey)) { //If we press space
			    catchingBarRB.AddForce(Vector2.up * catchingForce * Time.deltaTime, ForceMode2D.Force); //Add force to lift the bar
		    }
	    }

	    //If the fish is in our trigger box
	    if (inTrigger && reelingFish) {
		    catchPercentage += catchMultiplier * Time.deltaTime;
	    } else {
		    catchPercentage -= catchMultiplier * Time.deltaTime;
	    }
	    
	    //Changes fish from silhoutte to colour over time
	    var fishColor = Color.Lerp(Color.black, Color.white, Map(0, 100, 0, 1, catchPercentage));
	    fishBar.GetComponent<Image>().color = fishColor;
	    
	    //Clamps our percentage between 0 and 100
	    catchPercentage = Mathf.Clamp(catchPercentage, 0, 100);
	    catchProgressBar.value = catchPercentage;
	    if (catchPercentage >= 100) { //Fish is caught if percentage is full
		    catchPercentage = 0;
		    FishCaught();
	    }
    }
    
    //Called to cast our line
    private void CastLine() {
	    lineCast = true;
        
        // Di chuyển bóng thoại đến vị trí của người chơi
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            // Đặt Z = -5 để đảm bảo bóng thoại luôn nổi lên trên tất cả mọi thứ (không bị cây cối hay nhân vật che lấp)
            thoughtBubbles.transform.position = playerObj.transform.position + new Vector3(0, 1.5f, -5f);
            Debug.Log("[Fishing] Đã quăng cần! Chờ cá cắn câu... (Nhìn trên đầu nhân vật)");
        }

	    thoughtBubbles.SetActive(true);
	    StartCoroutine(WaitForNibble(10));
    }
    
    //Wait a random time to get a nibble
    private IEnumerator WaitForNibble(float maxWaitTime) {
	    yield return new WaitForSeconds(Random.Range(maxWaitTime * 0.25f, maxWaitTime)); //Wait between 25% of maxWaitTime and the maxWaitTime
	    thoughtBubbles.GetComponent<Animator>().SetTrigger("Alert"); //Show the alert thoughtbubble
	    nibble = true; 
	    StartCoroutine(LineBreak(2)); //If we dont respond in 2 seconds break the line
    }
    
    //Used to start the minigame
    private void StartReeling() {
	    reelingFish = true;
	    
	    nibble = false;
	    lineCast = false;
	    
	    //Set up the fish we are catching
	    currentFishOnLine = FishManager.GetRandomFishWeighted();
	    var tempSprite = Resources.Load<Sprite>($"FishSprites/{currentFishOnLine.spriteID}"); //Get fish sprite from our resources file
	    fishBar.GetComponent<Image>().sprite = tempSprite;

	    //Changes the width and height of the fishBar to accomodate for wider sprites
	    var w = Map(0, 32, 0, 100, tempSprite.texture.width);
	    var h = Map(0, 32, 0, 100, tempSprite.texture.height);
	    fishBar.GetComponent<RectTransform>().sizeDelta = new Vector2(w, h);
	    
	    minigameCanvas.SetActive(true);
    }
    
    //This breaks the line if we are waiting for a response too long
    private IEnumerator LineBreak(float lineBreakTime) {
	    yield return new WaitForSeconds(lineBreakTime);
	    Debug.Log("Line Broke!");
	    
	    //Disable thought bubbles
	    thoughtBubbles.GetComponent<Animator>().SetTrigger("Reset");
	    thoughtBubbles.SetActive(false);
	    
	    lineCast = false;
	    nibble = false;
    }

    //Called from the FishingMinigame_FishTrigger script
    public void FishInBar() {
	    inTrigger = true;
    }
    
	//Called from the FishingMinigame_FishTrigger script
    public void FishOutOfBar() {
	    inTrigger = false;
    }

    //Called when the catchpercentage hits 100
    public void FishCaught() {
	    reelingFish = false; //No longer reeling in a fish
	    //Reset the thought bubbles
	    thoughtBubbles.SetActive(false);
	    thoughtBubbles.GetComponent<Animator>().SetTrigger("Reset");
	    minigameCanvas.SetActive(false); //Disable the fishing canvas
	    catchingbar.transform.localPosition = catchingBarLoc; //Reset the catching bars position

        // --- KIỂM TRA TỶ LỆ CÂU LÊN THÚ HOANG DÃ ---
        if (possibleWaterBeasts != null && possibleWaterBeasts.Count > 0 && Random.Range(0f, 100f) <= beastEncounterChance)
        {
            BeastData randomBeast = possibleWaterBeasts[Random.Range(0, possibleWaterBeasts.Count)];
            if (randomBeast == null)
            {
                Debug.LogWarning("[Fishing] randomBeast is null! Vui lòng kiểm tra lại cấu hình possibleWaterBeasts trong FishingMinigame.");
                return;
            }

            Debug.Log($"[Fishing] Đã câu được một con thú: {randomBeast.beastName}!");

            if (battleTransferData != null)
            {
                // Truyền đội địch và ID quái sang BattleScene
                System.Collections.Generic.List<RuntimeBeastData> runtimeTeam = new System.Collections.Generic.List<RuntimeBeastData>();
                runtimeTeam.Add(new RuntimeBeastData(randomBeast, 1)); // Mặc định level 1

                battleTransferData.SetEnemyTeam(runtimeTeam);
                battleTransferData.originScene = BattleTransferData.OriginScene.Map;
                battleTransferData.isTrainerBattle = false;
                battleTransferData.isSingleBattle = false;
                battleTransferData.lastEncounteredBeastId = ""; // Không có trên map để xóa

                // Lưu lại vị trí người chơi
                GameObject playerObj = GameObject.FindWithTag("Player");
                if (playerObj != null)
                {
                    battleTransferData.lastPlayerPosition = playerObj.transform.position;
                    battleTransferData.returnToLastPosition = true;
                }

                GameSceneManager.GoToBattle();
            }
            else
            {
                Debug.LogWarning("[Fishing] Chưa gán BattleTransferData vào FishingMinigame!");
            }
            return; // Dừng lại, không xử lý câu cá nữa
        }
        // ------------------------------------------

	    if (currentFishOnLine == null) { //This picks a new fish if the old one is lost by chance
		    currentFishOnLine = FishManager.GetRandomFish();
	    }
	    Debug.Log($"Caught a: {currentFishOnLine.name}");
        
        // --- HIỆU ỨNG POPUP ---
        Sprite fishSprite = Resources.Load<Sprite>($"FishSprites/{currentFishOnLine.spriteID}");
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Vector3 spawnPos = player != null ? player.transform.position : transform.position;
        FishCatchEffect.Show(fishSprite, spawnPos, currentFishOnLine.name);
        // ----------------------

        // --- Cập nhật nhiệm vụ ---
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnFishCaught();
            
            // Thử lấy vật phẩm có tên giống con cá (ví dụ: Crab), hoặc rớt mặc định vật phẩm "Cá"
            Kinnly.Item fishItem = QuestManager.Instance.GetItemByName(currentFishOnLine.name);
            if (fishItem == null) fishItem = QuestManager.Instance.GetItemByName("Cá");
            
            if (fishItem != null && player != null)
            {
                Kinnly.PlayerInventory inv = player.GetComponent<Kinnly.PlayerInventory>();
                if (inv != null)
                {
                    inv.AddItem(fishItem, 1);
                    inv.SaveNow();
                    Debug.Log($"[Fishing] Đã tự động thêm 1 {fishItem.name} vào kho đồ!");
                }
            }
            else
            {
                Debug.LogWarning($"[Fishing] Không tìm thấy vật phẩm FishItem tên là '{currentFishOnLine.name}' hoặc 'Cá' để thêm vào kho!");
            }
        }
    }
    

    //Classic mapping script x
    private float Map(float a, float b, float c, float d, float x) {
	    return (x - a) / (b - a) * (d - c) + c;
    }
    
}
