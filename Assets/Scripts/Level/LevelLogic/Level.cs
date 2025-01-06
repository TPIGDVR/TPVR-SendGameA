using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour, IScriptLoadQueuer
{
    [Header("Death related")]
    [SerializeField]
    Transform deathPosition;
    [SerializeField] ScannerUI tryAgainScanner;

    [SerializeField]
    Room[] rooms;
    [SerializeField]
    Level_Door door;
    [SerializeField]
    Room startingRoom;
    
    Vector3 respawnPosition;
    public void Initialize()
    {
        rooms = GetComponentsInChildren<Room>();
        door = GetComponentInChildren<Level_Door>();
        EventSystem.level.TriggerEvent(LevelEvents.ENTER_NEW_ROOM, startingRoom);
        EventSystem.level.AddListener<ObjectiveName>(LevelEvents.OBJECTIVE_PROGRESSED, UpdateRespawnPosition);
        EventSystem.player.AddListener(PlayerEvents.DEATH_SCREEN_FADED, TeleportPlayerToDeathPosition);
        EventSystem.player.AddListener(PlayerEvents.RES_SCREEN_FADED, TeleportPlayerToRespawnPosition);
        UpdateRespawnPosition();
        //EventSystem.player.AddListener()
    }

    private void Awake()
    {
        ScriptLoadSequencer.Enqueue(this,(int)LevelLoadSequence.LEVELSYSTEM);
    }

    public void RestartGame()
    {
        EventSystem.player.TriggerEvent(PlayerEvents.RESTART);
    }

    public void UpdateRespawnPosition(ObjectiveName _ = ObjectiveName.KIOSK)
    {
        respawnPosition = GameData.playerTransform.position;
    }
    [ContextMenu("move player to death")]
    public void TeleportPlayerToDeathPosition()
    {
        print($"moving player to {deathPosition.position}");
        GameData.playerTransform.position = deathPosition.position;
        GameData.playerTransform.rotation = deathPosition.rotation;
    }

    public void TeleportPlayerToRespawnPosition()
    {
        //StartCoroutine(TeleportPlayerToLocation(respawnPosition));
        GameData.playerTransform.position = respawnPosition;
        tryAgainScanner.ResetScanner();
    }

    [ContextMenu("Complete Tutorial")]
    public void CompleteTutorial()
    {
        EventSystem.dialog.TriggerEvent(DialogEvents.ACTIVATE_HEARTRATE);
        EventSystem.dialog.TriggerEvent(DialogEvents.ACTIVATE_NOISE_INDICATOR);
        GameData.ChangeTutorialStatus(false);
    }
}
