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
    public bool isOver; //���ӿ��� Ȯ��

    [Header("------------[ Object Pooling ]")]
    public GameObject fruitPrefab;
    public Transform fruitGroup;
    public List<Fruit> fruitPool;
    //public GameObject effectPrefab;
    //public Transform effectGroup;
    //public List<ParticleSystem> effectPool;
    [Range(1, 30)] //�Ʒ� ������ �����̵��ٷ�
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
        Application.targetFrameRate = 60; //������ ����

        fruitPool = new List<Fruit>();
        //effectPool = new List<ParticleSystem> ();
        for (int i = 0; i < poolSize; i++) {
            MakeFruit();
        }

        if (!PlayerPrefs.HasKey("MaxScore")) { //�ְ������� ������ 0�� ����
            PlayerPrefs.SetInt("MaxScore", 0);
        }

        maxScoreText.text = PlayerPrefs.GetInt("MaxScore").ToString();        
    }

    public void GameStart()
    {
        //������Ʈ Ȱ��ȭ/��Ȱ��ȭ
        line.SetActive(true);
        bottom.SetActive(true);
        scoreText.gameObject.SetActive(true);
        NextFruitGroup.SetActive(true);
        maxScoreText.gameObject.SetActive(true);
        startGroup.SetActive(false);

        bgmPlayer.Play();
        SfxPlay(sfx.Button);

        //���� ����
        Invoke("NextFruit", 1.5f);
    }

    Fruit MakeFruit() //������ ���� �����ϴ� �Լ�
    {
        //����Ʈ ����
        /*GameObject instantEffectObj = Instantiate(effectPrefab, effectGroup); //�ν��Ͻ� �����Լ�
        instantEffectObj.name = "Effect " + effectPool.Count;
        ParticleSystem instantEffect = instantEffectObj.GetComponent<ParticleSystem>();
        effectPool.Add(instantEffect); //Ǯ ����Ʈ�� ����*/

        //���� ����
        GameObject instantFruitObj = Instantiate(fruitPrefab, fruitGroup); //�ν��Ͻ� �����Լ�
        instantFruitObj.name = "Fruit " + fruitPool.Count;
        Fruit instantFruit = instantFruitObj.GetComponent<Fruit>();
        instantFruit.manager = this;
        //instantFruit.effect = instantEffect;
        fruitPool.Add(instantFruit);

        return instantFruit;
    }

    Fruit GetFruit() //������Ʈ Ǯ���� ���� �������� �Լ�
    {
        for(int i = 0; i < fruitPool.Count; i++) {
            poolCursor = (poolCursor+1) % fruitPool.Count;
            if (!fruitPool[poolCursor].gameObject.activeSelf) { //��Ȱ��ȭ���°����� ������ ���� ���� ������
                return fruitPool[poolCursor];
            }
        }
        return MakeFruit(); //MakeFruit�� Fruit�� ��ȯ�ϹǷ� �Լ��� ��ȯ�ص� ��������
    }

    void NextFruit() //���� ���� �����Լ�
    {
        if (isOver) {
            return;
        }

        lastfruit = GetFruit();
        lastfruit.level = nextFruitCursor;
        nextFruitCursor = Random.Range(0, maxLevel); //0~7����
        nextFruitImage.sprite = fruitSprite[nextFruitCursor];
        lastfruit.gameObject.SetActive(true); //���� ���� �� Ȱ��ȭ

        SfxPlay(GameManager.sfx.Next);
        StartCoroutine(WaitNext()); //�ڷ�ƾ ����
    }

    IEnumerator WaitNext()
    {
        while (lastfruit != null) { 
            yield return null; //yield�� ���� 1������ ��ٸ�
        }

        yield return new WaitForSeconds(2.5f);

        NextFruit();
    }

    public void TouchDown()
    {
        if (lastfruit == null) { //�����ϱ� �� ��Ʈ�ѵǾ� �������� �� ����
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
        lastfruit = null; //���� ������ ��Ʈ�� X
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
        //1. ��� �ȿ� Ȱ��ȭ�� ��� ���� ��������
        Fruit[] fruits = GameObject.FindObjectsOfType<Fruit>();

        //2. ����� �� ��� ������ ����ȿ�� ��Ȱ��ȭ
        for (int i = 0; i < fruits.Length; i++) {
            fruits[i].rigid.simulated = false;
        }

        //3. 1���� ����� �ϳ��� �����ؼ� �����
        /*for (int i = 0; i < fruits.Length; i++) {
            fruits[i].Hide(Vector3.up * 100, false); //�Ұ����� ��ǥ�� �־� �������� ���� �и�
            yield return new WaitForSeconds(0.1f);
        }*/

        //yield return new WaitForSeconds(1f);

        //�ְ����� ����
        int maxScore = Mathf.Max(score, PlayerPrefs.GetInt("MaxScore"));
        PlayerPrefs.SetInt("MaxScore", maxScore);

        //���ӿ��� UI ǥ��
        subScoreText.text = "���� : " + scoreText.text;
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
        SceneManager.LoadScene(0);//0 �Ǵ� "Main"
    }

    public void SfxPlay(sfx type) //ȿ���� ���
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
        sfxCursor = (sfxCursor + 1) % sfxPlayer.Length; //������ �������� �迭 ���� �ʰ� ����
    }

    private void Update() //�ڷΰ��� �� ���� ����
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
