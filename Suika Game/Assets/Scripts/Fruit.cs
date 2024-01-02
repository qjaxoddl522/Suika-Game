using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fruit : MonoBehaviour
{
    public GameManager manager;
    //public ParticleSystem effect;
    public int level;
    public bool isDrag;
    public bool isMerge; //합쳐지고 있는지 확인
    //public bool isAttach; //최근에 부딫혔는지 확인

    public Rigidbody2D rigid;
    CircleCollider2D circle;
    Animator anim;
    SpriteRenderer spriteRenderer;

    float deadTime;
    public int[] levelUpScore;

    private void Awake() {
        rigid = GetComponent<Rigidbody2D>();
        circle = GetComponent<CircleCollider2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable() //스크립트가 활성화될때 실행
    {
        anim.SetInteger("Level", level); //쌍따옴표는 애니메이션의 파라미터
    }

    private void OnDisable() //오브젝트 재사용을 위한 비활성화시 초기화
    {
        level = 0;
        isDrag = false;
        isMerge = false;
        //isAttach = false;

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.zero;

        rigid.simulated = false;
        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = 0;
        circle.enabled = true;
    }

    void Update()
    {
        if (isDrag) {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //x축 경계 설정
            float leftBorder = -4.2f + transform.localScale.x / 2f; //테두리 두깨와 과일의 반지름 고려
            float rightBorder = 4.2f - transform.localScale.x / 2f;

            if (mousePos.x < leftBorder) {
                mousePos.x = leftBorder;
            }
            else if (mousePos.x > rightBorder) {
                mousePos.x = rightBorder;
            }

            mousePos.y = 8;
            mousePos.z = 0;
            transform.position = Vector3.Lerp(transform.position, mousePos, 0.2f);
        }
    }

    public void Drag() { //TouchPad->GameManager에서 실행
        isDrag = true;
    }

    public void Drop() {
        isDrag = false;
        rigid.simulated = true;
    }

    /*void OnCollisionEnter2D(Collision2D collision)
    {
        StartCoroutine(AttachRoutine());
    }

    IEnumerator AttachRoutine()
    {
        if (isAttach) {
            yield break;
        }
        isAttach = true;
        manager.SfxPlay(GameManager.sfx.Attach);

        yield return new WaitForSeconds(0.2f);

        isAttach = false;
    }*/

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Fruit") {
            Fruit other = collision.gameObject.GetComponent<Fruit>();

            if (level == other.level && !isMerge && !other.isMerge && level < 7) {
                // 자신과 상대 위치 가져오기
                float meX = transform.position.x;
                float meY = transform.position.y;
                float otherX = other.transform.position.x;
                float otherY = other.transform.position.y;

                //중점 구하기
                float midX = (meX + otherX) / 2;
                float midY = (meY + otherY) / 2;

                //1. 자신이 아래에 있을때
                //2. 동일한 높이일 때, 자신이 오른쪽에 있을때
                if (meY < otherY || (meY == otherY && meX > otherX)) {
                    //상대방은 숨김
                    other.Hide(new Vector3(midX, midY, 1), false);
                    //자신 레벨업
                    Hide(new Vector3(midX, midY, 0), true);
                }
            }
        }
    }

    public void Hide(Vector3 targetpos, bool levelUp)
    {
        isMerge = true;

        rigid.simulated = false; //물리효과 비활성화
        circle.enabled = false; //콜라이더 비활성화

        /*if(targetpos == Vector3.up * 100) { //게임오버 할 때 터지는 이펙트
            EffectPlay();
        }*/

        StartCoroutine(HideRoutine(targetpos, levelUp));
    }

    IEnumerator HideRoutine(Vector3 targetpos, bool levelUp)
    {
        int frameCount = 0;

        while (frameCount < 10) { //마치 Update처럼 로직 실행
            frameCount++;
            if (targetpos != Vector3.up * 100) { //게임오버 아니면
                transform.position = Vector3.Lerp(transform.position, targetpos, 0.7f);
            }
            else if (targetpos == Vector3.up * 100) {
                transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, 0.2f);
            }
            yield return null; //여러 프레임에 걸쳐 돌도록
        }

        if (levelUp) {
            manager.score += levelUpScore[level]; //점수 올리기

            anim.SetInteger("Level", level + 1); //애니메이터에 성장한 레벨값을 전달
            //EffectPlay();
            manager.SfxPlay(GameManager.sfx.LevelUp);

            level++;

            manager.maxLevel = Mathf.Max(level, manager.maxLevel); //Mathf.Max - 인자값중에 최대값 반환

            rigid.simulated = true; //물리효과 활성화
            circle.enabled = true; //콜라이더 활성화

            isMerge = false;
        }
        else {
            isMerge = false;
            gameObject.SetActive(false);
        }
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Finish") {
            deadTime += Time.deltaTime;

            if (deadTime > 2 ) {
                spriteRenderer.color = new Color(0.9f, 0.2f, 0.2f);
            }
            if (deadTime > 5) {
                manager.GameOver();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Finish") {
            deadTime = 0;
            spriteRenderer.color = Color.white;
        }
    }

    /*void EffectPlay()
    {
        effect.transform.position = transform.position; //위치지정
        effect.transform.localScale = transform.localScale; //스케일 맞추기
        effect.Play();
    }*/
}
