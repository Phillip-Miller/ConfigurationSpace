using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
/// <summary>
/// For use in the new C# input action manager
/// Here I will parse inputs (and allow/disallow according to boundries)
/// No camera movement for now
/// </summary>
public class NewController : MonoBehaviour
{
    public float joystickSensitivity;

    public MyScript myscript; 
    public Configuration configuration;
    [SerializeField] private InputActionReference flapActionReference;
    [SerializeField] private InputActionReference AButtonPress;
    [SerializeField] private InputActionReference interiorActionReference;




    float AngleD;
    float AngleC;
    float AngleA;
    float AngleB;    
    
    // Start is called before the first frame update
    void Start()
    {
        flapActionReference.action.performed += onFlapMovement;
        interiorActionReference.action.performed += onInteriorMovement;
        AButtonPress.action.performed += onAButton;
    }


    // Update is called once per frame
    void Update()
    {
        //change active configuration space
        
    }
    private void onAButton(InputAction.CallbackContext value)
    { //lock movement to once axis at once perhaps? espicially for the flaps.
        Debug.Log("Abutton");
    }
    private void onFlapMovement(InputAction.CallbackContext value)
    {
        //@FIXME a little tricky to only move on one axis at once maybe implement jump
        Debug.Log("onFlapMovement");
        Vector2 inputMovement = value.ReadValue<Vector2>();
        float aValue = Math.Abs(inputMovement.x) > .5 ? inputMovement.x : 0;
        float bValue = Math.Abs(inputMovement.y) > .5 ? inputMovement.y : 0;
        AngleA = myscript.UpdateAngleA(aValue);
        AngleB = myscript.UpdateAngleB(bValue);
        configuration.setPosition(new Vector4(AngleA, AngleB, AngleC, AngleD));

    }
    
    private void onInteriorMovement(InputAction.CallbackContext value)
    {
        Debug.Log("on interior Movement");
        Vector2 inputMovement = value.ReadValue<Vector2>();
        float dValue = inputMovement.x * joystickSensitivity;
        float cValue = inputMovement.y * joystickSensitivity;
        AngleD = myscript.UpdateAngleD(dValue);
        AngleC = myscript.UpdateAngleC(cValue);
        configuration.setPosition(new Vector4(AngleA, AngleB, AngleC, AngleD));
    }
}
