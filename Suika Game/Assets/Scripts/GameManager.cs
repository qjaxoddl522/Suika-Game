using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("------------[ Core ]")]
    public int score;
    public int maxLevel;
    public bool isOver; //게임오버 확인

    [Header("------------[ Object Pooling ]")]
    public GameObject fruitPrefab;
    public Transform fruitGroup;
    public List<Fruit> fruitPool;
    //public GameObject effectPrefab;
    //public Transform effectGroup;
    //public List<ParticleSystem> effectPool;
    [Range(1, 30)] //아래 변수를 슬라이딩바로
    public int poolSize;
    public int poolCursor;
    public Fruit lastfruit;

    [Header("------------[ Audio ]")]
    public AudioSource bgmPlayer;
    public AudioSource[] sfxPlayer;
    public AudioClip[] sfxClip;
    public enum sfx { LevelUp, Next, Attach, Button, Over};
    int sfxCursor;

    [Header("------------[ UI ]")]
    public GameObject startGroup;
    public GameObject endGroup;
    public GameObject NextFruitGroup;
    public Text scoreText;
    public Text maxScoreText;
    public Text subScoreText;
    public Sprite[] fruitSprite;
    public Image nextFruitImage;
    public int nextFruitCursor;

    [Header("------------[ ETC ]")]
    public GameObject line;
    public GameObject bottom;

    private void Awake()
    {
        Application.targetFrameRate = 60; //프레임 고정

        fruitPool = new List<Fruit>();
        //effectPool = new List<ParticleSystem> ();
        for (int i = 0; i < poolSize; i++) {
            MakeFruit();
        }

        if (!PlayerPrefs.HasKey("MaxScore")) { //최고점수가 없으면 0점 저장
            PlayerPrefs.SetInt("MaxScore", 0);
        }

        maxScoreText.text = PlayerPrefs.GetInt("MaxScore").ToString();        
    }

    public void GameStart()
    {
        //오브젝트 활성화/비활성화
        line.SetActive(true);
        bottom.SetActive(true);
        scoreText.gameObject.SetActive(true);
        NextFruitGroup.SetActive(true);
        maxScoreText.gameObject.SetActive(true);
        startGroup.SetActive(false);

        bgmPlayer.Play();
        SfxPlay(sfx.Button);

        //게임 시작
        Invoke("NextFruit", 1.5f);
    }

    Fruit MakeFruit() //과일을 새로 생성하는 함수
    {
        //이펙트 생성
        /*GameObject instantEffectObj = Instantiate(effectPrefab, effectGroup); //인스턴스 생성함수
        instantEffectObj.name = "Effect " + effectPool.Count;
        ParticleSystem instantEffect = instantEffectObj.GetComponent<ParticleSystem>();
        effectPool.Add(instantEffect); //풀 리스트에 저장*/

        //과일 생성
        GameObject instantFruitObj = Instantiate(fruitPrefab, fruitGroup); //인스턴스 생성함수
        instantFruitObj.name = "Fruit " + fruitPool.Count;
        Fruit instantFruit = instantFruitObj.GetComponent<Fruit>();
        instantFruit.manager = this;
        //instantFruit.effect = instantEffect;
        fruitPool.Add(instantFruit);

        return instantFruit;
    }

    Fruit GetFruit() //오브젝트 풀에서 과일 가져오는 함수
    {
        for(int i = 0; i < fruitPool.Count; i++) {
            poolCursor = (poolCursor+1) % fruitPool.Count;
            if (!fruitPool[poolCursor].gameObject.activeSelf) { //비활성화상태과일이 있으면 현재 과일 내보냄
                return fruitPool[poolCursor];
            }
        }
        return MakeFruit(); //MakeFruit도 Fruit를 반환하므로 함수를 반환해도 문제없다
    }

    void NextFruit() //다음 과일 지정함수
    {
        if (isOver) {
            return;
        }

        lastfruit = GetFruit();
        lastfruit.level = nextFruitCursor;
        nextFruitCursor = Random.Range(0, maxLevel); //0~7까지
        nextFruitImage.sprite = fruitSprite[nextFruitCursor];
        lastfruit.gameObject.SetActive(true); //레벨 설정 후 활성화

        SfxPlay(GameManager.sfx.Next);
        StartCoroutine(WaitNext()); //코루틴 실행
    }

    IEnumerator WaitNext()
    {
        while (lastfruit != null) { 
            yield return null; //yield를 통해 1프레임 기다림
        }

        yield return new WaitForSeconds(2.5f);

        NextFruit();
    }

    public void TouchDown()
    {
        if (lastfruit == null) { //시작하기 전 컨트롤되어 에러나는 것 방지
            return;
        }
        lastfruit.Drag();
    }
    public void TouchUp()
    {
        if (lastfruit == null) {
            return;
        }
        lastfruit.Drop();
        lastfruit = null; //떨군 과일은 컨트롤 X
    }

    public void GameOver()
    {
        if (isOver) {
            return;
        }

        isOver = true;

        StartCoroutine(GameOverRoutine());
    }

    IEnumerator GameOverRoutine()
    {
        //1. 장면 안에 활성화된 모든 과일 가져오기
        Fruit[] fruits = GameObject.FindObjectsOfType<Fruit>();

        //2. 지우기 전 모든 과일의 물리효과 비활성화
        for (int i = 0; i < fruits.Length; i++) {
            fruits[i].rigid.simulated = false;
        }

        //3. 1번의 목록을 하나씩 접근해서 지우기
        /*for (int i = 0; i < fruits.Length; i++) {
            fruits[i].Hide(Vector3.up * 100, false); //불가능한 좌표를 넣어 합쳐지는 경우와 분리
            yield return new WaitForSeconds(0.1f);
        }*/

        //yield return new WaitForSeconds(1f);

        //최고점수 갱신
        int maxScore = Mathf.Max(score, PlayerPrefs.GetInt("MaxScore"));
        PlayerPrefs.SetInt("MaxScore", maxScore);

        //게임오버 UI 표시
        subScoreText.text = "점수 : " + scoreText.text;
        endGroup.SetActive(true);

        bgmPlayer.Stop();
        SfxPlay(sfx.Over);

        yield return null;
    }

    public void Reset()
    {
        SfxPlay(sfx.Button);
        StartCoroutine(ResetCoroutine());
    }

    IEnumerator ResetCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(0);//0 또는 "Main"
    }

    public void SfxPlay(sfx type) //효과음 재생
    {
        switch (type) {
            case sfx.LevelUp:
                sfxPlayer[sfxCursor].clip = sfxClip[Random.Range(0, 3)];
                break;
            case sfx.Next:
                sfxPlayer[sfxCursor].clip = sfxClip[3];
                break;
            case sfx.Attach:
                sfxPlayer[sfxCursor].clip = sfxClip[4];
                break;
            case sfx.Button:
                sfxPlayer[sfxCursor].clip = sfxClip[5];
                break;
            case sfx.Over:
                sfxPlayer[sfxCursor].clip = sfxClip[6];
                break;
        }
        sfxPlayer[sfxCursor].Play();
        sfxCursor = (sfxCursor + 1) % sfxPlayer.Length; //나머지 연산으로 배열 길이 초과 방지
    }

    private void Update() //뒤로가기 시 게임 종료
    {
        if (Input.GetButtonDown("Cancel")) {
            Application.Quit();
        }
    }

    private void LateUpdate()
    {
        scoreText.text = score.ToString();
    }
}
