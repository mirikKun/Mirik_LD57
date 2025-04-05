using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.Scripts.Infrastracture
{
    public class SceneLoaderService
    {


        
        public static void InstantLoad(string name)
        {
            SceneManager.LoadScene(name);
        }
    
 
    }
}