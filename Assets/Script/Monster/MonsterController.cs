﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MonsterController : MonoBehaviour
{
    private readonly float DistPerTile = 1f;
    private float MaxHP;
    private float HP;
    private float SpeedOrgin;
    private float Speed;
    private int Attack;
    private List<Point> MovePath;
    private float dist = 0f;
    private float AllDist = 0f;
    private int index = 1;
    public int reward = 1;

    private int icedLevel = 0;

    public Point Position
    {
        get
        {
            return MovePath[index];
        }
    }
    public List<TowerManager> TargetedTowers { get; set; }
    public virtual void SetStatus(float maxHP, float speed, int attack,int reward, List<Point> path)
    {
        this.MaxHP = maxHP;
        this.HP = maxHP;
        this.Speed = speed;
        this.SpeedOrgin = Speed;
        this.Attack = attack;
        this.reward = reward;
        this.MovePath = new List<Point>(path);
    }
    public void DestroyObj()
    {
        GameManager.Inst.Monsters.Remove(this);
        foreach (var t in TargetedTowers)
            t.target = null;
        Destroy(gameObject);
    }
    public void Start()
    {
        TargetedTowers = new List<TowerManager>();
        var startPoint = this.MovePath[0];
        transform.position = startPoint.ToVector() + (this.MovePath[1] - startPoint).ToVector() / 2 + GameManager.REVISE;
    }
    public void FixedUpdate()
    {
        //이동 거리만큼 dist와 AllDist 증가
        dist += this.Speed * Time.deltaTime;
        AllDist += this.Speed * Time.deltaTime;
        //
        if (dist >= DistPerTile)
        {
            //경로의 끝에 도달하면 GameManager에 도착을 알리고 오브젝트 파괴
            if (++index == MovePath.Count - 1)
            {
                GameManager.Inst.MonsterArrive(this.Attack);
                DestroyObj();
                return;
            }
            dist -= DistPerTile;
        }
        Point prevDir = MovePath[index] - MovePath[index - 1]; //이전 타일의 이동 방향
        ConversionDir(ref prevDir);
        Point nextDir = MovePath[index + 1] - MovePath[index]; //현재 타일의 이동 방향
        ConversionDir(ref nextDir);
        Vector3 position = MovePath[index].ToVector() + GameManager.REVISE; //현재 타일의 중점
        //이동 거리만큼 조정 후 이동
        if (dist <= DistPerTile / 2)
            position -= (DistPerTile / 2 - dist) * prevDir.ToVector3();
        else
            position += (dist - DistPerTile / 2) * nextDir.ToVector3();
        transform.position = position;
        //
    }
    private void ConversionDir(ref Point p)  //끝점에서 끝점으로 이동한 경우 올바른 방향값으로 변환
    {
        if (Math.Abs(p.x) > 1)
            p.x /= -8;
        else if (Math.Abs(p.y) > 1)
            p.y /= -8;
    }
    public void AttackedByTower(float Damage)
    {
        HP -= Damage;

        DrawHitEffect();

        if(HP <= 0.0f)
        {
            GameManager.Inst.MonsterArrive(0);
            DestroyObj();
        }
    }

    public void DrawHitEffect()
    {
        SpriteRenderer spr = GetComponent<SpriteRenderer>();

        Color color = spr.color;
        color.a = HP / MaxHP;
        spr.color = color;
    }

    public void Iced(int tier)
    {
        if (icedLevel >= tier) return;

        icedLevel = tier;
        Speed = SpeedOrgin * (1 - 0.25f * tier);

        //Debug.Log("Iced! : " + icedLevel);
        Debug.Log("Speed : " + Speed);
    }
}