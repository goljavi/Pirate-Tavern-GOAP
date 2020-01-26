using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    enum DefaultDropdown
    {
        DontCheck,
        True,
        False,
    }

    enum CoinDropdown
    {
        DontCheck,
        N100,
        N50,
        N0,
    }

    enum DrunkennessDropdown
    {
        DontCheck,
        P0,
        P10,
        P30,
    }

    enum EquipmentDropdown
    {
        DontCheck,
        ElfBlood,
        BreakingObject,
        Key,
        Cabinet,
        None,
    }

    enum PresetsDropdown
    {
        None,
        StealElfBloodBreakingCabinet,
        StealElfBloodStealingCabinetKey,
        StealCabinetAndDie,
        StealCabinetWhilePeopleDrunk,
        StealElfBloodWinningCabinetKey,
        StealElfBloodWinningCabinetKeyDrunk,
    }

    public Dropdown hasEscaped;
    public Dropdown coin;
    public Dropdown alive;
    public Dropdown drunkenness;
    public Dropdown equipment;
    public Dropdown brokenCabinet;
    public Dropdown angryOwner;
    public Dropdown cabinetOpen;
    public Dropdown playDarts;

    public Dropdown presets;

    public Button run;
    public Button restart;

    public GameObject main;

    public event Action<Preferences<WorldState>> OnRunButtonPressed;

    // Update is called once per frame
    Preferences<WorldState> MakePreferences()
    {
        var preferences = new Preferences<WorldState>();

        // Has Escaped
        switch ((DefaultDropdown)hasEscaped.value)
        {
            case DefaultDropdown.True:
                preferences += (WorldState wrld) => wrld.hasEscaped == true;
                break;
            case DefaultDropdown.False:
                preferences += (WorldState wrld) => wrld.hasEscaped == false;
                break;
            default:
                break;
        }

        // Coin
        switch ((CoinDropdown)coin.value)
        {
            case CoinDropdown.N100:
                preferences += (WorldState wrld) => wrld.coin == 100;
                break;
            case CoinDropdown.N50:
                preferences += (WorldState wrld) => wrld.coin <= 50;
                break;
            case CoinDropdown.N0:
                preferences += (WorldState wrld) => wrld.coin == 0;
                break;
            default:
                break;
        }

        // Alive
        switch ((DefaultDropdown)alive.value)
        {
            case DefaultDropdown.True:
                preferences += (WorldState wrld) => wrld.alive == true;
                break;
            case DefaultDropdown.False:
                preferences += (WorldState wrld) => wrld.alive == false;
                break;
            default:
                break;
        }

        // Drunkenness
        switch ((DrunkennessDropdown)drunkenness.value)
        {
            case DrunkennessDropdown.P0:
                preferences += (WorldState wrld) => wrld.drunkenness == 0;
                break;
            case DrunkennessDropdown.P10:
                preferences += (WorldState wrld) => wrld.drunkenness >= 0.1f;
                break;
            case DrunkennessDropdown.P30:
                preferences += (WorldState wrld) => wrld.drunkenness >= 0.3f;
                break;
            default:
                break;
        }

        // Equipment
        switch ((EquipmentDropdown)equipment.value)
        {
            case EquipmentDropdown.ElfBlood:
                preferences += (WorldState wrld) => wrld.equipment == Equipment.ElfBlood;
                break;
            case EquipmentDropdown.BreakingObject:
                preferences += (WorldState wrld) => wrld.equipment == Equipment.BreakingObject;
                break;
            case EquipmentDropdown.Key:
                preferences += (WorldState wrld) => wrld.equipment == Equipment.Key;
                break;
            case EquipmentDropdown.Cabinet:
                preferences += (WorldState wrld) => wrld.equipment == Equipment.Cabinet;
                break;
            case EquipmentDropdown.None:
                preferences += (WorldState wrld) => wrld.equipment == Equipment.None;
                break;
            default:
                break;
        }

        // Broken Cabinet
        switch ((DefaultDropdown)brokenCabinet.value)
        {
            case DefaultDropdown.True:
                preferences += (WorldState wrld) => wrld.brokenCabinet == true;
                break;
            case DefaultDropdown.False:
                preferences += (WorldState wrld) => wrld.brokenCabinet == false;
                break;
            default:
                break;
        }

        // Angry Owner
        switch ((DefaultDropdown)angryOwner.value)
        {
            case DefaultDropdown.True:
                preferences += (WorldState wrld) => wrld.angryOwner == true;
                break;
            case DefaultDropdown.False:
                preferences += (WorldState wrld) => wrld.angryOwner == false;
                break;
            default:
                break;
        }

        // Cabinet Open
        switch ((DefaultDropdown)cabinetOpen.value)
        {
            case DefaultDropdown.True:
                preferences += (WorldState wrld) => wrld.cabinetOpen == true;
                break;
            case DefaultDropdown.False:
                preferences += (WorldState wrld) => wrld.cabinetOpen == false;
                break;
            default:
                break;
        }

        // Play Darts
        switch ((DefaultDropdown)playDarts.value)
        {
            case DefaultDropdown.True:
                preferences += (WorldState wrld) => wrld.playDarts == true;
                break;
            case DefaultDropdown.False:
                preferences += (WorldState wrld) => wrld.playDarts == false;
                break;
            default:
                break;
        }

        return preferences;
    }

    public void OnPresetSelected()
    {
        SetAllDropdownsToDefault();
        switch ((PresetsDropdown)presets.value)
        {
            case PresetsDropdown.StealElfBloodBreakingCabinet:
                hasEscaped.value = (int)DefaultDropdown.True;
                angryOwner.value = (int)DefaultDropdown.False;
                alive.value = (int)DefaultDropdown.True;
                break;
            case PresetsDropdown.StealElfBloodStealingCabinetKey:
                hasEscaped.value = (int)DefaultDropdown.True;
                brokenCabinet.value = (int)DefaultDropdown.False;
                angryOwner.value = (int)DefaultDropdown.True;
                break;
            case PresetsDropdown.StealCabinetAndDie:
                hasEscaped.value = (int)DefaultDropdown.True;
                brokenCabinet.value = (int)DefaultDropdown.False;
                angryOwner.value = (int)DefaultDropdown.False;
                alive.value = (int)DefaultDropdown.False;
                break;
            case PresetsDropdown.StealCabinetWhilePeopleDrunk:
                hasEscaped.value = (int)DefaultDropdown.True;
                brokenCabinet.value = (int)DefaultDropdown.False;
                angryOwner.value = (int)DefaultDropdown.False;
                alive.value = (int)DefaultDropdown.True;
                //drunkenness.value = (int)DrunkennessDropdown.P30;
                break;
            case PresetsDropdown.StealElfBloodWinningCabinetKey:
                hasEscaped.value = (int)DefaultDropdown.True;
                brokenCabinet.value = (int)DefaultDropdown.False;
                angryOwner.value = (int)DefaultDropdown.False;
                playDarts.value = (int)DefaultDropdown.True;
                break;
            case PresetsDropdown.StealElfBloodWinningCabinetKeyDrunk:
                hasEscaped.value = (int)DefaultDropdown.True;
                brokenCabinet.value = (int)DefaultDropdown.False;
                angryOwner.value = (int)DefaultDropdown.False;
                playDarts.value = (int)DefaultDropdown.True;
                drunkenness.value = (int)DrunkennessDropdown.P10;
                break;
            default:
                break;
        }
    }

    void SetAllDropdownsToDefault()
    {
        hasEscaped.value = 0;
        coin.value = 0;
        alive.value = 0;
        drunkenness.value = 0;
        equipment.value = 0;
        brokenCabinet.value = 0;
        angryOwner.value = 0;
        cabinetOpen.value = 0;
        playDarts.value = 0;
    }

    public void RunButtonPressed()
    {
        run.GetComponentInChildren<Text>().text = "Loading...";
        OnRunButtonPressed(MakePreferences());
    }

    public void ShowRestart()
    {
        Hide();
        restart.gameObject.SetActive(true);
    }

    public void OnRestartButtonPressed() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    public void Hide() => main.SetActive(false);
}
