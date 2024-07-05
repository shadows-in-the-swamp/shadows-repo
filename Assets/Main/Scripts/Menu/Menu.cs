using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

public class Menu : MonoBehaviour
{ 
    [SerializeField] protected GameObject _principalMenu;
    [SerializeField] protected GameObject _credits;
    [SerializeField] protected GameObject _controller;

   public void PlayGame()
   {
     CargaNivel.NivelCarga((int)SceneIndexes.Chapter1);
   }
   public void Test()
   {
     CargaNivel.NivelCarga((int)SceneIndexes.Test);
   }
   public void Exit()
   { 
      Application.Quit();
   }
   public void Credits()
   {
     _principalMenu.SetActive(false);
     _credits.SetActive(true);
   }
   public void Awake()
   {
     PrincipalMenu();
   }
   public void PrincipalMenu()
   {
     _principalMenu.SetActive(true);
     _credits.SetActive(false);
     _controller.SetActive(false);
   } 
   public void Controller()
   {
     _principalMenu.SetActive(false);
     _controller.SetActive(true);
   }
   
}
