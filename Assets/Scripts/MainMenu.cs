using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    #region VARIABLES
    #endregion VARIABLES

    #region METHODS
    #region PUBLIC

    // Load the game scene
    public void StartGame ()
    {
        SceneManager.LoadScene(1);
    }

    #endregion PUBLIC

    #region PRIVATE

    // Start is called before the first frame update
    private void Start ()
    {
        
    }

    #endregion PRIVATE
    #endregion METHODS
}
