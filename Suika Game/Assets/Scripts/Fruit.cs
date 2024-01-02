using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fruit : MonoBehaviour
{
    public GameManager manager;
    //public ParticleSystem effect;
    public int level;
    public bool isDrag;
    public bool isMerge; //�������� �ִ��� Ȯ��
    //public bool isAttach; //�ֱٿ� �΋H������ Ȯ��

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

    private void OnEnable() //��ũ��Ʈ�� Ȱ��ȭ�ɶ� ����
    {
        anim.SetInteger("Level", level); //�ֵ���ǥ�� �ִϸ��̼��� �Ķ����
    }

    private void OnDisable() //������Ʈ ������ ���� ��Ȱ��ȭ�� �ʱ�ȭ
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
            //x�� ��� ����
            float leftBorder = -4.2f + transform.localScale.x / 2f; //�׵θ� �α��� ������ ������ ���
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

    public void Drag() { //TouchPad->GameManager���� ����
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
                // �ڽŰ� ��� ��ġ ��������
                float meX = transform.position.x;
                float meY = transform.position.y;
                float otherX = other.transform.position.x;
                float otherY = other.transform.position.y;

                //���� ���ϱ�
                float midX = (meX + otherX) / 2;
                float midY = (meY + otherY) / 2;

                //1. �ڽ��� �Ʒ��� ������
                //2. ������ ������ ��, �ڽ��� �����ʿ� ������
                if (meY < otherY || (meY == otherY && meX > otherX)) {
                    //������ ����
                    other.Hide(new Vector3(midX, midY, 1), false);
                    //�ڽ� ������
                    Hide(new Vector3(midX, midY, 0), true);
                }
            }
        }
    }

    public void Hide(Vector3 targetpos, bool levelUp)
    {
        isMerge = true;

        rigid.simulated = false; //����ȿ�� ��Ȱ��ȭ
        circle.enabled = false; //�ݶ��̴� ��Ȱ��ȭ

        /*if(targetpos == Vector3.up * 100) { //���ӿ��� �� �� ������ ����Ʈ
            EffectPlay();
        }*/

        StartCoroutine(HideRoutine(targetpos, levelUp));
    }

    IEnumerator HideRoutine(Vector3 targetpos, bool levelUp)
    {
        int frameCount = 0;

        while (frameCount < 10) { //��ġ Updateó�� ���� ����
            frameCount++;
            if (targetpos != Vector3.up * 100) { //���ӿ��� �ƴϸ�
                transform.position = Vector3.Lerp(transform.position, targetpos, 0.7f);
            }
            else if (targetpos == Vector3.up * 100) {
                transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, 0.2f);
            }
            yield return null; //���� �����ӿ� ���� ������
        }

        if (levelUp) {
            manager.score += levelUpScore[level]; //���� �ø���

            anim.SetInteger("Level", level + 1); //�ִϸ����Ϳ� ������ �������� ����
            //EffectPlay();
            manager.SfxPlay(GameManager.sfx.LevelUp);

            level++;

            manager.maxLevel = Mathf.Max(level, manager.maxLevel); //Mathf.Max - ���ڰ��߿� �ִ밪 ��ȯ

            rigid.simulated = true; //����ȿ�� Ȱ��ȭ
            circle.enabled = true; //�ݶ��̴� Ȱ��ȭ

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
        effect.transform.position = transform.position; //��ġ����
        effect.transform.localScale = transform.localScale; //������ ���߱�
        effect.Play();
    }*/
}
