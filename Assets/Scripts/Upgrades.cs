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
    [SerializeField] private float _orb_cash_0;     // Price to pay to upgrade to this level
    [SerializeField] private float _orb_RS_0;       // Time before laser can shoot again
    [SerializeField] private int _orb_LPC_0;        // Number of lasers fired per charge

    [Header("Level 1")]
    [SerializeField] private float _orb_cash_1;
    [SerializeField] private float _orb_RS_1;
    [SerializeField] private int _orb_LPC_1;
    
    [Header("Level 2")]
    [SerializeField] private float _orb_cash_2;
    [SerializeField] private float _orb_RS_2;
    [SerializeField] private int _orb_LPC_2;

    [Header("Level 3")]
    [SerializeField] private float _orb_cash_3;
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
    [SerializeField] private float _comp_cash_0;    // Price to pay to upgrade to this level
    [SerializeField] private float _comp_RS_0;      // Time before laserbeam shoots again
    [SerializeField] private float _comp_LD_0;      // Time during which laser is active and shooting

    [Header("Level 1")]
    [SerializeField] private float _comp_cash_1;
    [SerializeField] private float _comp_RS_1;  
    [SerializeField] private float _comp_LD_1;

    [Header("Level 2")]
    [SerializeField] private float _comp_cash_2;
    [SerializeField] private float _comp_RS_2;
    [SerializeField] private float _comp_LD_2;

    [Header("Level 3")]
    [SerializeField] private float _comp_cash_3;
    [SerializeField] private float _comp_RS_3;
    [SerializeField] private float _comp_LD_3;

    // These hold references to the corresponding level structures
    public readonly List<ComplexityLevel> _ComplexityLevels = new List<ComplexityLevel>();
    private ComplexityLevel _ComplexityLevel0;
    private ComplexityLevel _ComplexityLevel1;
    private ComplexityLevel _ComplexityLevel2;
    private ComplexityLevel _ComplexityLevel3;

    [Space(10)]
    [Header("Pulsar")]
    [Header("Level 0")]
    [SerializeField] private float _puls_cash_0;    // Price to pay to upgrade to this level
    [SerializeField] private float _puls_RS_0;      // Time before pulsar pulses again
    //[SerializeField] private float _puls_RoS_0;   // Speed at which pulsar rotates around the station (should've expected it...)
    [SerializeField] private float _puls_BR_0;      // Size of the pulse destroying enemies

    [Header("Level 1")]
    [SerializeField] private float _puls_cash_1;
    [SerializeField] private float _puls_RS_1;
    //[SerializeField] private float _puls_RoS_1;
    [SerializeField] private float _puls_BR_1;

    [Header("Level 2")]
    [SerializeField] private float _puls_cash_2;
    [SerializeField] private float _puls_RS_2;
    //[SerializeField] private float _puls_RoS_2;
    [SerializeField] private float _puls_BR_2;

    [Header("Level 3")]
    [SerializeField] private float _puls_cash_3;
    [SerializeField] private float _puls_RS_3;
    //[SerializeField] private float _puls_RoS_3;
    [SerializeField] private float _puls_BR_3;

    // These hold references to the corresponding level structures
    public readonly List<PulsarLevel> _PulsarLevels = new List<PulsarLevel>();
    private PulsarLevel _PulsarLevel0;
    private PulsarLevel _PulsarLevel1;
    private PulsarLevel _PulsarLevel2;
    private PulsarLevel _PulsarLevel3;

    [Space(10)]
    [Header("Small shield")]
    [Header("Level 0")]
    [SerializeField] private float _ss_cash_0;      // Price to pay to upgrade to this level
    [SerializeField] private float _ss_mh_0;        // Shield's max health
    [SerializeField] private float _ss_cd_0;        // Time before shield comes back up after being destroyed

    [Header("Level 1")]
    [SerializeField] private float _ss_cash_1;
    [SerializeField] private float _ss_mh_1;
    [SerializeField] private float _ss_cd_1;

    [Header("Level 2")]
    [SerializeField] private float _ss_cash_2;
    [SerializeField] private float _ss_mh_2;
    [SerializeField] private float _ss_cd_2;

    [Header("Level 3")]
    [SerializeField] private float _ss_cash_3;
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
    [SerializeField] private float _ls_cash_0;      // Price to pay to upgrade to this level
    [SerializeField] private float _ls_mh_0;        // Shield's max health
    [SerializeField] private float _ls_cd_0;        // Time before shield comes back up after being destroyed

    [Header("Level 1")]
    [SerializeField] private float _ls_cash_1;
    [SerializeField] private float _ls_mh_1;
    [SerializeField] private float _ls_cd_1;

    [Header("Level 2")]
    [SerializeField] private float _ls_cash_2;
    [SerializeField] private float _ls_mh_2;
    [SerializeField] private float _ls_cd_2;

    [Header("Level 3")]
    [SerializeField] private float _ls_cash_3;
    [SerializeField] private float _ls_mh_3;
    [SerializeField] private float _ls_cd_3;

    // These hold references to the corresponding level structures
    public readonly List<ShieldLevel> _LShieldLevels = new List<ShieldLevel>();
    private ShieldLevel _LShieldLevel0;
    private ShieldLevel _LShieldLevel1;
    private ShieldLevel _LShieldLevel2;
    private ShieldLevel _LShieldLevel3;

    [Space(10)]
    [Header("Station")]
    [Header("Level 0")]
    [SerializeField] private float _st_cash_0;      // Price to pay to upgrade to this level
    [SerializeField] private float _st_rep_0;       // Price to pay to repair 100 station health
    [SerializeField] private float _st_mh_0;        // Station's max health
    [SerializeField] private float _st_ra_0;        // Station's health gained per repair tick
    [SerializeField] private float _st_rs_0;        // Station's repair speed

    [Header("Level 1")]
    [SerializeField] private float _st_cash_1;
    [SerializeField] private float _st_mh_1;
    [SerializeField] private float _st_ra_1;
    [SerializeField] private float _st_rs_1;

    [Header("Level 2")]
    [SerializeField] private float _st_cash_2;
    [SerializeField] private float _st_mh_2;
    [SerializeField] private float _st_ra_2;
    [SerializeField] private float _st_rs_2;

    [Header("Level 3")]
    [SerializeField] private float _st_cash_3;
    [SerializeField] private float _st_mh_3;
    [SerializeField] private float _st_ra_3;
    [SerializeField] private float _st_rs_3;

    // These hold references to the corresponding level structures
    public readonly List<StationLevel> _StationLevels = new List<StationLevel>();
    private StationLevel _StationLevel0;
    private StationLevel _StationLevel1;
    private StationLevel _StationLevel2;
    private StationLevel _StationLevel3;

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

    // Structure holding all Complexity parameters per level
    public struct ComplexityLevel
    {
        public readonly float _rechargeSpeed;       // Time before laser can shoot again
        public readonly float _laserBeamDuration;   // Time during which laser beam is active

        // Constructor
        public ComplexityLevel (float recSpeed, float laserDur)
        {
            _rechargeSpeed = recSpeed;
            _laserBeamDuration = laserDur;
        }
    }

    // Structure holding all Pulsar parameters per level
    public struct PulsarLevel
    {
        public readonly float _rechargeSpeed;       // Time before laser can shoot again
        //public readonly float _rotationSpeed;     // Speed at which the pulsar rotates around the station
        public readonly float _blastRadius;         // Radius of the pulse destroying enemies

        // Constructor
        //public PulsarLevel (float recSpeed, float rotSpeed, float blast)
        public PulsarLevel (float recSpeed, float blast)
        {
            _rechargeSpeed = recSpeed;
            //_rotationSpeed = rotSpeed;
            _blastRadius = blast;
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

    // Structure holding all Station parameters per level
    public struct StationLevel
    {
        public readonly float _maxHealth;       // Station's max health
        public readonly float _repairAmount;    // Amount health for each repair/heal tick
        public readonly float _repairSpeed;     // Time before shield comes back up after being destroyed

        // Constructor
        public StationLevel (float maxHealth,float amount, float repairSpeed)
        {
            _maxHealth = maxHealth;
            _repairAmount = amount;
            _repairSpeed = repairSpeed;
        }
    }

    // Enum containing  upgrade prices
    public enum Type
    {
        DEFENSEORB,
        COMPLEXITY,
        PULSAR,
        S_SHIELD,
        L_SHIELD,
        STATION,
        REPAIR
    }

    // List of all the prices for each upgrade of each type
    public readonly Dictionary<Type, float[]> _Prices = new Dictionary<Type, float[]>();

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

        // Initialize the _Prices dictionary for other scripts to refer too
        InitializeUpgradePrices();
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

        // Complexities
        _ComplexityLevel0 = new ComplexityLevel(_comp_RS_0, _comp_LD_0);
        _ComplexityLevels.Add(_ComplexityLevel0);
        _ComplexityLevel1 = new ComplexityLevel(_comp_RS_1, _comp_LD_1);
        _ComplexityLevels.Add(_ComplexityLevel1);
        _ComplexityLevel2 = new ComplexityLevel(_comp_RS_2, _comp_LD_2);
        _ComplexityLevels.Add(_ComplexityLevel2);
        _ComplexityLevel3 = new ComplexityLevel(_comp_RS_3, _comp_LD_3);
        _ComplexityLevels.Add(_ComplexityLevel3);

        // Pulsar
        //_PulsarLevel0 = new PulsarLevel(_puls_RS_0, _puls_RoS_0, _puls_BR_0);
        _PulsarLevel0 = new PulsarLevel(_puls_RS_0, _puls_BR_0);
        _PulsarLevels.Add(_PulsarLevel0);
        //_PulsarLevel1 = new PulsarLevel(_puls_RS_1, _puls_RoS_1, _puls_BR_1);
        _PulsarLevel1 = new PulsarLevel(_puls_RS_1, _puls_BR_1);
        _PulsarLevels.Add(_PulsarLevel1);
        //_PulsarLevel2 = new PulsarLevel(_puls_RS_2, _puls_RoS_2, _puls_BR_2);
        _PulsarLevel2 = new PulsarLevel(_puls_RS_2, _puls_BR_2);
        _PulsarLevels.Add(_PulsarLevel2);
        //_PulsarLevel3 = new PulsarLevel(_puls_RS_3, _puls_RoS_3, _puls_BR_3);
        _PulsarLevel3 = new PulsarLevel(_puls_RS_3, _puls_BR_3);
        _PulsarLevels.Add(_PulsarLevel3);

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

        // Station
        _StationLevel0 = new StationLevel(_st_mh_0,_st_ra_0 , _st_rs_0);
        _StationLevels.Add(_StationLevel0);
        _StationLevel1 = new StationLevel(_st_mh_1, _st_ra_1, _st_rs_1);
        _StationLevels.Add(_StationLevel1);
        _StationLevel2 = new StationLevel(_st_mh_2, _st_ra_2, _st_rs_2);
        _StationLevels.Add(_StationLevel2);
        _StationLevel3 = new StationLevel(_st_mh_3, _st_ra_3, _st_rs_3);
        _StationLevels.Add(_StationLevel3);
    }

    // Populate the _Prices Dictionary with all the prices for each upgrade of each type
    private void InitializeUpgradePrices ()
    {
        float[] orbPrices = new float[] { _orb_cash_0, _orb_cash_1, _orb_cash_2, _orb_cash_3 };
        _Prices.Add(Type.DEFENSEORB, orbPrices);
        float[] compPrices = new float[] { _comp_cash_0, _comp_cash_1, _comp_cash_2, _comp_cash_3 };
        _Prices.Add(Type.COMPLEXITY, compPrices);
        float[] pulsPrices = new float[] { _puls_cash_0, _puls_cash_1, _puls_cash_2, _puls_cash_3 };
        _Prices.Add(Type.PULSAR, pulsPrices);
        float[] ssPrices = new float[] { _ss_cash_0, _ss_cash_1, _ss_cash_2, _ss_cash_3 };
        _Prices.Add(Type.S_SHIELD, ssPrices);
        float[] lsPrices = new float[] { _ls_cash_0, _ls_cash_1, _ls_cash_2, _ls_cash_3 };
        _Prices.Add(Type.L_SHIELD, lsPrices);
        float[] stPrices = new float[] { _st_cash_0, _st_cash_1, _st_cash_2, _st_cash_3 };
        _Prices.Add(Type.STATION, stPrices);
        // Repair is a bit particular, as there is only one price
        float[] repairPrice = new float[] { _st_rep_0 };
        _Prices.Add(Type.REPAIR, repairPrice);
    }

    #endregion METHODS
}
