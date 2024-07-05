using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

public static class CargaNivel 
{
 public static int siguienteNivel;
 public static void NivelCarga(int nombre)
 {
    siguienteNivel = nombre;
    SceneManager.LoadScene((int)SceneIndexes.Loading);
 }
}
