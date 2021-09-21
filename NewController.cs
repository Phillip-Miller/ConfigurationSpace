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
    }
    private void onFlapMovement(InputAction.CallbackContext value)
    {  
        Vector2 inputMovement = value.ReadValue<Vector2>();
        float aValue = inputMovement.x;
        float bValue = inputMovement.y;

        if (Math.Abs(aValue) > Math.Abs(bValue)) //only move one flap at a time
            bValue = 0;
        else
            aValue = 0;

        AngleA = myscript.UpdateAngleA(aValue);
        AngleB = myscript.UpdateAngleB(bValue);
        configuration.setPosition(new Vector4(AngleA, AngleB, AngleC, AngleD));

    }
    
    private void onInteriorMovement(InputAction.CallbackContext value)
    {
        Vector2 inputMovement = value.ReadValue<Vector2>();
        float dValue = inputMovement.x * joystickSensitivity;
        float cValue = inputMovement.y * joystickSensitivity;
        if(Math.Abs(dValue) > Math.Abs(cValue)) //only move one flap at a time
            cValue = 0;
        else
            dValue = 0;
        AngleD = Math.Abs(dValue) > .01? myscript.UpdateAngleD(dValue) : myscript.UpdateAngleD(0); //@FIXME could be updating with a value of 0 or nah idk
        AngleC = Math.Abs(cValue) > .01 ? myscript.UpdateAngleC(cValue) : myscript.UpdateAngleC(0);


        configuration.setPosition(new Vector4(AngleA, AngleB, AngleC, AngleD));
    }
}
