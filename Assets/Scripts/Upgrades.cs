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
    public readonly List<DefenseOrbLevel> _OrbLevels = new List<DefenseOrbLevel> () ;
    private DefenseOrbLevel _OrbLevel0;
    private DefenseOrbLevel _OrbLevel1;
    private DefenseOrbLevel _OrbLevel2;
    private DefenseOrbLevel _OrbLevel3;

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
    [SerializeField] private float _ss_mh_0;    // Shield's max health
    [SerializeField] private float _ss_cd_0;    // Time before shield comes back up after being destroyed

    [Header("Level 1")]
    [SerializeField] private float _ss_mh_1;
    [SerializeField] private float _ss_cd_1;

    [Header("Level 2")]
    [SerializeField] private float _ss_mh_2;
    [SerializeField] private float _ss_cd_2;

    [Header("Level 3")]
    [SerializeField] private float _ss_mh_3;
    [SerializeField] private float _ss_cd_3;

    // These hold references to the corresponding level structures
    public readonly List<ShieldLevel> _SShieldLevels = new List<ShieldLevel>();
    private ShieldLevel _SShieldLevel0;
    private ShieldLevel _SShieldLevel1;
    private ShieldLevel _SShieldLevel2;
    private ShieldLevel _SShieldLevel3;

    [Space(10)]
    [Header("Large shield")]
    [Header("Level 0")]
    [SerializeField] private float _ls_mh_0;    // Shield's max health
    [SerializeField] private float _ls_cd_0;    // Time before shield comes back up after being destroyed

    [Header("Level 1")]
    [SerializeField] private float _ls_mh_1;
    [SerializeField] private float _ls_cd_1;

    [Header("Level 2")]
    [SerializeField] private float _ls_mh_2;
    [SerializeField] private float _ls_cd_2;

    [Header("Level 3")]
    [SerializeField] private float _ls_mh_3;
    [SerializeField] private float _ls_cd_3;

    // These hold references to the corresponding level structures
    public readonly List<ShieldLevel> _LShieldLevels = new List<ShieldLevel>();
    private ShieldLevel _LShieldLevel0;
    private ShieldLevel _LShieldLevel1;
    private ShieldLevel _LShieldLevel2;
    private ShieldLevel _LShieldLevel3;

    #endregion UPGRADE_VALUES

    #region STRUCTURES

    // Structure holding all DefenseOrb parameters per level
    public struct DefenseOrbLevel
    {
        public readonly float _rechargeSpeed;   // Time before laser can shoot again
        public readonly int _lasersPerCharge;   // Number of lasers fired per charge

        // Constructor
        public DefenseOrbLevel (float recSpeed, int lasers)
        {
            _rechargeSpeed = recSpeed;
            _lasersPerCharge = lasers;
        }
    }

    // Structure holding all Shield parameters per level
    public struct ShieldLevel               
    {
        public readonly float _maxHealth;   // Shield's max health
        public readonly float _cooldown;    // Time before shield comes back up after being destroyed

        // Constructor
        public ShieldLevel (float maxHealth, float cd)
        {
            _maxHealth = maxHealth;
            _cooldown = cd;
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

        // Small shield
        _SShieldLevel0 = new ShieldLevel(_ss_mh_0, _ss_cd_0);
        _SShieldLevels.Add(_SShieldLevel0);
        _SShieldLevel1 = new ShieldLevel(_ss_mh_1, _ss_cd_1);
        _SShieldLevels.Add(_SShieldLevel1);
        _SShieldLevel2 = new ShieldLevel(_ss_mh_2, _ss_cd_2);
        _SShieldLevels.Add(_SShieldLevel2);
        _SShieldLevel3 = new ShieldLevel(_ss_mh_3, _ss_cd_3);
        _SShieldLevels.Add(_SShieldLevel3);

        // Large shield
        _LShieldLevel0 = new ShieldLevel(_ls_mh_0, _ss_cd_0);
        _LShieldLevels.Add(_LShieldLevel0);
        _LShieldLevel1 = new ShieldLevel(_ls_mh_1, _ss_cd_1);
        _LShieldLevels.Add(_LShieldLevel1);
        _LShieldLevel2 = new ShieldLevel(_ls_mh_2, _ss_cd_2);
        _LShieldLevels.Add(_LShieldLevel2);
        _LShieldLevel3 = new ShieldLevel(_ls_mh_3, _ss_cd_3);
        _LShieldLevels.Add(_LShieldLevel3);
    }

    #endregion METHODS
}
