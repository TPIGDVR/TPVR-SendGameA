using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour, IScriptLoadQueuer
{
    [SerializeField]
    Transform deathPosition;

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
        EventSystem.level.AddListener(LevelEvents.OBJECTIVE_PROGRESSED, UpdateRespawnPosition);
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

    public void UpdateRespawnPosition()
    {
        respawnPosition = GameData.playerTransform.position;
    }

    public void TeleportPlayerToDeathPosition()
    {
        GameData.playerTransform.position = deathPosition.position;
    }

    public void TeleportPlayerToRespawnPosition()
    {
        GameData.playerTransform.position = respawnPosition;

    }

    [ContextMenu("Complete Tutorial")]
    public void CompleteTutorial()
    {
        EventSystem.dialog.TriggerEvent(DialogEvents.ACTIVATE_HEARTRATE);

        EventSystem.dialog.TriggerEvent(DialogEvents.ACTIVATE_NOISE_INDICATOR);
    }
}
