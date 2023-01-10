using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Upgrades : MonoBehaviour
{
    #region VARIABLES
    #region SINGLETON DECLARATION

    private static Upgrades _instance;
    public static Upgrades GetInstance () { return _instance; }

    #endregion SINGLETON DECLARATION

    #region UPGRADE_VALUES

    // This is to allow balancing directly through the Unity editor
    [Header("Defense Orbs")]
    [Header("Level 0")]
    [SerializeField] private float _orb_RS_0;   // Time before laser can shoot again
    [SerializeField] private int _orb_LPC_0;    // Number of lasers fired per charge

    [Header("Level 1")]
    [SerializeField] private float _orb_RS_1;
    [SerializeField] private int _orb_LPC_1;
    
    [Header("Level 2")]
    [SerializeField] private float _orb_RS_2;
    [SerializeField] private int _orb_LPC_2;

    [Header("Level 3")]
    [SerializeField] private float _orb_RS_3;
    [SerializeField] private int _orb_LPC_3;

    // These hold references to the corresponding level structures
    public List<DefenseOrbLevel> _OrbLevels = new List<DefenseOrbLevel> () ;
    public DefenseOrbLevel _OrbLevel0;
    public DefenseOrbLevel _OrbLevel1;
    public DefenseOrbLevel _OrbLevel2;
    public DefenseOrbLevel _OrbLevel3;

    [Space(10)]
    [Header("Complexities")]
    [Header("Level 0")]
    [SerializeField] private float compelexity;

    [Space(10)]
    [Header("Pulsar")]
    [Header("Level 0")]
    [SerializeField] private float pulsar;

    [Space(10)]
    [Header("Station")]
    [Header("Level 0")]
    [SerializeField] private float health;

    [Space(10)]
    [Header("Small shield")]
    [Header("Level 0")]
    [SerializeField] private float health_small;
    [SerializeField] private float rechargerate;

    [Space(10)]
    [Header("Large shield")]
    [Header("Level 0")]
    [SerializeField] private float health_large;
    [SerializeField] private float recharge_rate2;

    #endregion UPGRADE_VALUES

    #region STRUCTURES

    public struct DefenseOrbLevel           // Structure holding all DefenseOrb parameters per level
    {
        public readonly float _rechargeSpeed;        // Time before laser can shoot again
        public readonly int _lasersPerCharge;        // Number of lasers fired per charge

        // Constructor
        public DefenseOrbLevel (float recSpeed, int lasers)
        {
            _rechargeSpeed = recSpeed;
            _lasersPerCharge = lasers;
        }
    }

    #endregion STRUCTURES
    #endregion VARIABLES

    #region METHODS
    #region UNITY

    // Instantiate singleton reference on Awake
    private void Awake ()
    {
        // Create singleton instance reference for other scripts to call
        if (_instance != null) {
            Debug.LogWarning("More than one instance of Upgrades script running");
        }
        _instance = this;
    }

    // Called after Awake but before other scripts' Start methods (they might need to upgrade structures)
    private void OnEnable ()
    {
        // Initialize upgrade level structures
        InitializeUpgradeLevels();
    }

    #endregion UNITY

    #region PUBLIC

    #endregion PUBLIC

    #region PRIVATE

    #endregion PRIVATE

    // Create strutures and lists containing the upgrade levels for all weapons
    // /!\ I need to do this because Unity doesn't allow editor serialization for structure type variables
    // Feels pretty convoluted, I know
    private void InitializeUpgradeLevels ()
    {
        // Defense Orbs
        _OrbLevel0 = new DefenseOrbLevel(_orb_RS_0, _orb_LPC_0);
        _OrbLevels.Add(_OrbLevel0);
        _OrbLevel1 = new DefenseOrbLevel(_orb_RS_1, _orb_LPC_1);
        _OrbLevels.Add(_OrbLevel1);
        _OrbLevel2 = new DefenseOrbLevel(_orb_RS_2, _orb_LPC_2);
        _OrbLevels.Add(_OrbLevel2);
        _OrbLevel3 = new DefenseOrbLevel(_orb_RS_3, _orb_LPC_3);
        _OrbLevels.Add(_OrbLevel3);
    }

    #endregion METHODS
}
