using UnityEngine;

public class Scene1 : MonoBehaviour
{
    private GameObject elfArcher;
    private GameObject footMan;
    void Start()
    {
        // ElfArcher 생성
        elfArcher = new GameObject("ElfArcher");
        elfArcher.AddComponent<ElfArcher>();  // ElfArcher 컴포넌트 추가 (자동으로 스프라이트 로드)
        elfArcher.transform.position = new Vector3(-122, 0, 0);
        
        // FootMan 생성
        footMan = new GameObject("FootMan");
        footMan.AddComponent<FootMan>();  // FootMan 컴포넌트 추가 (자동으로 스프라이트 로드)
        footMan.transform.position = new Vector3(122, 0, 0);

        FrameController.Instance.Add( this.onPlayAnimation, this);

    }

    private void onPlayAnimation()
    {
        elfArcher.GetComponent<BaseHero>().Execute();
        //footMan.GetComponent<BaseHero>().Execute();
    }
}
