using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

using Firebase;
using Firebase.Auth;
using Firebase.Functions;


public class AuthManager : MonoBehaviour
{

    //Firebase variables
    [Header("Firebase")]

    public DependencyStatus dependencyStatus;

    public FirebaseAuth auth; 
    public FirebaseFunctions functions;   
    public FirebaseUser User;

    //Login variables
    [Header("Login")]
    public GameObject loginGroup;
    public TMP_InputField emailLoginField;
    public TMP_InputField passwordLoginField;
    public TMP_Text errorLoginText;

    //Register variables
    [Header("Register")]
    public GameObject signupGroup;
    public TMP_InputField usernameField;
    public TMP_InputField emailSignupField;
    public TMP_InputField passwordSignupField;
    public TMP_InputField passwordSignupVerifyField;
    public TMP_Text errorSignupText;

    private bool showLogin = true;

    void Awake() {


        signupGroup.SetActive(false);

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            
            if (dependencyStatus == DependencyStatus.Available) {
                auth = FirebaseAuth.DefaultInstance;
                functions = FirebaseFunctions.DefaultInstance;
            }else{
                Debug.LogError("Missing Firebase Dependencies: " + dependencyStatus);
            }
        });
    }

    //MARK: Login Functionality

    public void LoginButton() {
        string email = emailLoginField.text;
        string password = passwordLoginField.text;
        StartCoroutine(Login(email, password));
    }

    private IEnumerator Login(string _email, string _password)
    {
        //Call the Firebase auth signin function passing the email and password
        var LoginTask = auth.SignInWithEmailAndPasswordAsync(_email, _password);
        //Wait until the task completes
        yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);

        if (LoginTask.Exception != null)
        {
            //If there are errors handle them
            Debug.LogWarning(message: $"Failed to register task with {LoginTask.Exception}");
            FirebaseException firebaseEx = LoginTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            string message = "Login Failed!";
            switch (errorCode)
            {
                case AuthError.MissingEmail:
                    message = "Missing Email";
                    break;
                case AuthError.MissingPassword:
                    message = "Missing Password";
                    break;
                case AuthError.WrongPassword:
                    message = "Wrong Password";
                    break;
                case AuthError.InvalidEmail:
                    message = "Invalid Email";
                    break;
                case AuthError.UserNotFound:
                    message = "Account does not exist";
                    break;
            }
            errorLoginText.text = message;
        }else{
            //User is now logged in
            //Now get the result
            User = LoginTask.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})", User.DisplayName, User.Email);
            errorLoginText.text = "";
            errorLoginText.text = "Logged In";
        }
    }

    //MARK: Signup Functionality

        //MARK: Signup Functionality

        // public void SignupButton() {
    //     string email = emailRegisterField.text;
    //     string password = passwordRegisterField.text;
    //     string passwordVerify = passwordRegisterVerifyField.text;
    //     StartCoroutine(Signup(email, password, passwordVerify));
    // }

        //MARK: Signup Functionality

    public void ToggleMode() {
        showLogin = !showLogin;

        loginGroup.SetActive(showLogin);
        signupGroup.SetActive(!showLogin);
        // loginGroup.GetComponent<CanvasGroup>().alpha = showLogin ? 1.0f : 0.0f;
        // signupGroup.GetComponent<CanvasGroup>().alpha = showLogin ? 0.0f : 1.0f;
    }

    public void SignupButton() {
        string username = usernameField.text;
        string email = emailSignupField.text;
        string password = passwordSignupField.text;
        string passwordVerify = passwordSignupVerifyField.text;
        StartCoroutine(Signup(username, email, password, passwordVerify));
    }

    private Task<string> signupTask(string email, string password, string username) {
        // Create the arguments to the callable function.
        var data = new Dictionary<string, object>();
        data["username"] = username;
        data["email"] = email;
        data["password"] = password;
        // Call the function and extract the operation from the result.
        var function = functions.GetHttpsCallable("createUser");
        return function.CallAsync(data).ContinueWith((task) => {
            // return (string) task.Result.Data;
            return "Done";
        });
    }

    private void resetFields() {
        errorLoginText.text = "";
        emailLoginField.text = "";
        passwordLoginField.text = "";
        usernameField.text = "";
        emailSignupField.text = "";
        passwordSignupField.text = "";
        passwordSignupVerifyField.text = "";
        
    }


    private IEnumerator Signup(string username, string email, string password, string passwordVerify) {

        if (email == "") {
            errorSignupText.text = "Missing Username";
        }else if(passwordSignupField.text != passwordSignupVerifyField.text) {
            errorSignupText.text = "Password Does Not Match!";
        }else{
            var task = signupTask(email, password, username);

            yield return new WaitUntil(predicate: () => task.IsCompleted);

            if (task.Exception != null) {
                Debug.LogWarning(message: $"Failed to register task with {task.Exception}");
                errorSignupText.text = $"Error: {task.Exception}";
            }else{
                Debug.LogWarning(message: "Registration successful");
                resetFields();
                ToggleMode();
            }
        }
    }
}
