
using UnityEngine;

public class PlayerSons : MonoBehaviour
{
    public AudioSource audioSource;

    public AudioClip somPulo;
    public AudioClip somLateral;

    public void TocarPulo()
    {
        audioSource.PlayOneShot(somPulo);
    }

    public void TocarLateral()
    {
        audioSource.PlayOneShot(somLateral);
    }
}
