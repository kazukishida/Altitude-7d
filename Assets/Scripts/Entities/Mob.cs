﻿using UnityEngine;
using System.Collections;

[RequireComponent (typeof (BoxCollider2D))]

public class Mob : Character {

	public enum MobBehavior { IdleStill, IdleMove, SeekAndAttack, FlySeek }
	
	public MobBehavior mobBehavior = MobBehavior.IdleMove;
	public bool ignoreOwnLayerCollision = true;
	public float seekSpeedBoostMultiplier = 1.5f; 
	public float behaviorTick = 2.0f;
	public float behaviorTickPercentRange = 0.1f;
	
	public int scoreValue = 100;
	
	public string mobName = "Solemn Simulacrum";
	
	private float timer = 0;
	
	private RaycastHit2D[] seenFront;
	private RaycastHit2D[] seenBack;
	

	
	protected override void Start(){
		base.Start();
		Spawn();
		
		if(ignoreOwnLayerCollision){
			Physics2D.IgnoreLayerCollision(gameObject.layer, gameObject.layer);
		}
	}
	
	protected override void Update(){
		CheckInspectorValues();
	
		if(isAlive){
			CheckInspectorValues();
			
			seenFront = CheckFront();
			seenBack = CheckBack();
			
			_rightGroundCheck = CheckGround (collider2D.bounds.size.x * 0.5f);
			_leftGroundCheck = CheckGround (-collider2D.bounds.size.x * 0.5f);
								
			timer += Time.deltaTime;
			float tickMorph = Random.Range(behaviorTick * (1.0f - behaviorTickPercentRange), behaviorTick * (1.0f + behaviorTickPercentRange));
			if(timer > tickMorph){
				if(mobBehavior == MobBehavior.IdleMove){
					TriggerIdleMove(false);
				} else if(mobBehavior == MobBehavior.SeekAndAttack){
					TriggerIdleMove(true);
				} else if(mobBehavior == MobBehavior.FlySeek){
				
				}
			}
		} else {
			
		}
	}
	
	public override void Die(){
		base.Die();
		_scoreManagerRef.AddScore(scoreValue);
	}
	
	protected void TriggerIdleMove(bool isSeeking){
		int direction = Random.Range(DIR_BACK,DIR_FRONT + 1);

		GameObject goFront = IsPlayerSeen(seenFront);
		GameObject goBack = IsPlayerSeen(seenBack);
		int chaseDirection = 0;
		if((goFront != null || goBack != null) && isSeeking){
			animator.SetBool("seenPlayer", true);
			if(goFront != null){
				chaseDirection = DIR_FRONT;
			} else if(goBack != null){
				chaseDirection = DIR_BACK;
			}
			StartCoroutine(MoveRoutine(chaseDirection, true));
			timer = 0;
		} else if(direction == DIR_BACK && _leftGroundCheck || direction == DIR_FRONT && _rightGroundCheck){
			animator.SetBool("seenPlayer", false);
			StartCoroutine(MoveRoutine (direction));
			timer = 0;
		}
	}
	
	protected GameObject IsPlayerSeen(RaycastHit2D[] side){
		foreach(RaycastHit2D col in side){
			if(col.collider.CompareTag("Player")){
				return col.collider.gameObject;
			}
		}
		return null;
	}
	
	IEnumerator MoveRoutine(int direction){
		return MoveRoutine(direction, false);
	}
	
	IEnumerator MoveRoutine(int direction, bool ignorePlatformEdges){
		float time = 0;
		while(time < 1.0f && isAlive){
			_movementVector = Vector2.zero;
			if(((direction == DIR_BACK && _leftGroundCheck) || (direction == DIR_FRONT && _rightGroundCheck)) || ignorePlatformEdges){
				if(ignorePlatformEdges){
					Move(direction, moveSpeed * seekSpeedBoostMultiplier);
				} else {
					if((IsPlayerSeen(seenFront) != null || IsPlayerSeen(seenBack) != null) && mobBehavior == MobBehavior.SeekAndAttack){
						break;
					} else {
						Move(direction, moveSpeed);
					}
				}
				ExecuteVector();
			} else {
				break;
			}
			time += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}	
	}
	
	protected override void CheckInspectorValues(){
		base.CheckInspectorValues();
		if(behaviorTickPercentRange < 0){
			behaviorTickPercentRange = 0;
		}
		if(behaviorTickPercentRange > 1.0f){
			behaviorTickPercentRange = 1.0f;
		}
		if(seekSpeedBoostMultiplier < 0){
			seekSpeedBoostMultiplier = 0;
		}
		if(seekSpeedBoostMultiplier > 20.0f){
			seekSpeedBoostMultiplier = 20.0f;
		}
	}
}
