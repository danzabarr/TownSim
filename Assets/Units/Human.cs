using System.Collections;
using System.Collections.Generic;
using TownSim.Items;
using UnityEngine;

namespace TownSim.Units
{
    public class Human : Unit
    {
        public ItemMode Mode
        {
            get
            {
                ItemType type = HeldType;
                return type == null ? ItemMode.Empty : type.mode;
            }
        }

        public Inventory OffHand { get; private set; }
        public Inventory Inventory { get; private set; }

        private int heldIndex;

        public int HeldIndex
        {
            get => heldIndex;
            set
            {
                value = Mathf.Clamp(value, 0, Inventory.Length);
                if (value != heldIndex)
                {
                    heldIndex = value;
                    UpdateHeldItem();
                }
            }
        }

        public ItemType HeldType => Inventory.Item(heldIndex);
        public int HeldQuantity => Inventory.Quantity(heldIndex);

        [Header("Pick Up Item")]
        public Vector3 pickupOffset;
        public float pickupRadius;
        public LayerMask pickupMask;
        public Transform hand_right;
        public Transform hand_left;
        public Transform shield;
        public Transform shield_holster;
        private Item pickupItem;
        private float pickupTime;
        private float pickupTimer;
        public AnimationCurve pickupScaleFunction;
        public AnimationCurve pickupPositionFunction;

        private List<ITarget> meleeHits;
        private bool meleeDamageActive;

        [Header("Ranged Attack")]
        public float rangedAttackSpeed;
        public Vector3 rangedAttackOrigin;
        public float rangedAttackMinimumVelocity;
        public float rangedAttackMaximumVelocity;
        public float rangedAttackRadius;
        public LayerMask rangedAttackMask;
        public Projectile rangedAttackProjectilePrefab;
        [Range(0, 1)]
        public float rangedDistanceAttenuation;
        [Range(0, 1)]
        public float rangedDirectionAttenuation;
        public Transform arrowBone;
        public GameObject arrowObject;
        private Vector3 rangedTarget;
        private bool attackGround;
        [Range(0, 1)]
        public float missedShotFrequency;
        public float missedShotDeviationRange;

        [Header("Blocking")]
        public bool blocking;
        public float blockingSpeedModifier;

        [Header("Misc")]
        public float rotationSpeed;
        public GameObject[] toolBones;
        public GameObject[] heldObjects;
        public GameObject[] wornObjects;
        public int inventorySpace;
        public bool pickaxeActive;
        public bool axeActive;

        public ITarget target;

        public bool IsRunning => Animator.GetBool("running");
        public bool MeleeDamageActive => currentHealth > 0 && meleeDamageActive && CurrentAction == Action.Melee;
        public bool PickaxeActive => currentHealth > 0 && pickaxeActive && CurrentAction == Action.Mine && target == null;
        public bool AxeActive => currentHealth > 0 && axeActive && CurrentAction == Action.Chop && target == null;

        protected override void Awake()
        {
            base.Awake();
            OffHand = new Inventory(1);
            OffHand.changed += HeldItemChanged;

            Inventory = new Inventory(inventorySpace);
            Inventory.changed += HeldItemChanged;
        }

        private void Start()
        {
            Inventory.Add(ItemManager.Type("Sword"), 10);
            OffHand.Add(ItemManager.Type("Shield"), 10);

            Animator.SetFloat("shootSpeed", rangedAttackSpeed);
            Animator.SetFloat("hitSpeed", hitSpeed);

            RuntimeAnimatorController rac = Animator.runtimeAnimatorController;
            foreach (AnimationClip clip in rac.animationClips)
            {
                if (clip.name == "Pickup")        //If it has the same name as your clip
                    pickupTime = clip.length;
            }
        }

        public void UpdateHeldItem()
        {
            ItemType mhType = HeldType;
            ItemType ohType = OffHand.Item(0);

            string mhMeshName = mhType?.meshName;
            string ohMeshName = ohType?.meshName;

            string mhBoneName = mhType?.boneName;
            string ohBoneName = ohType?.boneName;

            foreach (GameObject go in heldObjects)
                go.SetActive((mhMeshName != null && mhMeshName == go.name) || (ohMeshName != null && ohMeshName == go.name));

            foreach (GameObject go in toolBones)
                go.gameObject.SetActive((mhBoneName != null && mhBoneName == go.name) || (ohBoneName != null && ohBoneName == go.name));

            if (mhType == null || mhType.mode != ItemMode.Sword)
                shield.SetParent(shield_holster, false);
            else
                shield.SetParent(hand_left, false);
        }

        public void HeldItemChanged(object source, Inventory.InventoryChangedEventArgs args)
        {
            if (source == Inventory && args.index != heldIndex)
                return;

            UpdateHeldItem();
        }

        public void Pickup()
        {
            if (currentHealth <= 0)
                return;

            if (CurrentAction != Action.Idle)
                return;

            Vector3 pickupPoint = transform.TransformPoint(pickupOffset);

            Collider[] colliders = Physics.OverlapSphere(pickupPoint, pickupRadius, pickupMask, QueryTriggerInteraction.UseGlobal);

            if (colliders.Length > 0)
            {
                Item pi = null;
                float sqDistance = float.MaxValue;

                for (int i = 0; i < colliders.Length; i++)
                {
                    Item item = colliders[i].GetComponentInParent<Item>();
                    if (item == null)
                        continue;

                    if (item.pickedUp)
                        continue;

                    if (Inventory.SpaceFor(item.type) < item.quantity)
                        continue;

                    float d = Vector3.SqrMagnitude(item.transform.position - pickupPoint);
                    if (d < sqDistance)
                    {
                        pi = item;
                        sqDistance = d;
                    }
                }

                if (pi != null)
                {
                    CurrentAction = Action.PickUp;
                    pickupItem = pi;
                    pickupItem.pickedUp = true;
                    pickupItem.DisableColliders();
                    pickupItem.DisableRigidbody();
                    Animator.SetBool("running", false);
                    Animator.SetTrigger("pickup");
                }
            }
        }

        public void Mine()
        {
            if (currentHealth <= 0)
                return;

            if (CurrentAction != Action.Idle)
                return;

            if (Mode != ItemMode.Pickaxe)
                return;

            Animator.SetBool("running", false);
            Animator.SetTrigger("mine");
            CurrentAction = Action.Mine;
            target = null;
            pickaxeActive = false;
        }

        public void Chop()
        {
            if (currentHealth <= 0)
                return;

            if (CurrentAction != Action.Idle)
                return;

            if (Mode != ItemMode.Axe)
                return;

            Animator.SetBool("running", false);
            Animator.SetTrigger("chop");
            CurrentAction = Action.Chop;
            target = null;
            axeActive = false;
        }

        public bool MeleeAttack(float speed, float targetRadius = 1f, float targetDirectionAtten = 1f, float targetDistanceAtten = .5f, bool attackNothing = true)
        {
            if (currentHealth <= 0)
                return false;

            if (CurrentAction != Action.Idle)
                return false;

            Vector3 strikeOrigin = Center();// transform.TransformPoint(meleeStrikeOrigin);
            Vector3 strikeDirection = transform.forward;// transform.TransformDirection(meleeStrikeDirection.normalized);

            ITarget nearest = null;
            float score = float.MinValue;

            //foreach (Collider c in Physics.OverlapSphere(strikeOrigin, meleeStrikeRadius, meleeStrikeMask, QueryTriggerInteraction.Ignore))
            //{
            //    ITarget t = c.GetComponentInParent<ITarget>();
            //
            //
            //
            //  if (t == null)
            //      continue;

            foreach (Unit t in Units())
            {

                if (t.Equals(this))
                    continue;

                if (t.CurrentHealth() <= 0)
                    continue;

                if (t.Faction() == Faction())
                    continue;

                Vector3 strikeTarget = t.Center();
                Vector3 delta = strikeTarget - strikeOrigin;

                float sqDist = Vector3.SqrMagnitude(delta);

                if (sqDist > targetRadius * targetRadius)
                    continue;

                float directionalAttenuation = Mathf.Max(0, Vector3.Dot(strikeDirection, delta.normalized));
                float proximityAttenuation = Mathf.Clamp(1f / sqDist, 0, 1);

                float d = Mathf.Lerp(1, directionalAttenuation, targetDirectionAtten) * Mathf.Lerp(1, proximityAttenuation, targetDistanceAtten);

                if (d > score)
                {
                    nearest = t;
                    score = d;
                }
            }
            if (attackNothing || nearest != null)
            {
                MeleeAttack(nearest, speed);
                return true;
            }
            return false;
        }

        public bool MeleeAttack(ITarget target, float speed)
        {
            if (currentHealth <= 0)
                return false;

            if (CurrentAction != Action.Idle)
                return false;

            this.target = target;
            CurrentAction = Action.Melee;
            Animator.SetBool("running", false);
            Animator.SetFloat("meleeSpeed", speed);
            Animator.SetTrigger("melee");
            meleeHits = new List<ITarget>();
            meleeDamageActive = false;
            return true;
        }

        private void MineStart()
        {
            pickaxeActive = true;
        }

        private void ChopStart()
        {
            axeActive = true;
        }

        private void MeleeStart()
        {
            meleeDamageActive = true;
        }


        private void Idle()
        {
            CurrentAction = Action.Idle;
            target = null;
            meleeDamageActive = false;
            pickaxeActive = false;
            axeActive = false;
        }

        public bool InRange(Vector3 point)
        {
            Vector3 origin = arrowBone.TransformPoint(rangedAttackOrigin);

            return (point - origin).sqrMagnitude < rangedAttackRadius * rangedAttackRadius;
        }

        public bool InRange(ITarget target)
        {
            return InRange(target.Center());
        }

        public bool RangedTarget(out Unit target)
        {
            Vector3 strikeOrigin = arrowBone.TransformPoint(rangedAttackOrigin);
            Vector2 strikeDirection = transform.forward.xz().normalized;

            target = null;
            float score = float.MinValue;

            foreach (Unit unit in Units())
            {
                if (unit == this)
                    continue;

                if (unit.CurrentHealth() <= 0)
                    continue;

                if (unit.Faction() == Faction())
                    continue;

                Vector3 strikeTarget = unit.Center();
                Vector3 delta = strikeTarget - strikeOrigin;
                float sqDist = Vector3.SqrMagnitude(delta);

                if (sqDist > rangedAttackRadius * rangedAttackRadius)
                    continue;

                float directionalAttenuation = Vector2.Dot(strikeDirection, delta.xz().normalized);

                if (directionalAttenuation < .95f)
                    continue;

                float proximityAttenuation = Mathf.Clamp(1f / sqDist, 0, 1);

                float d = Mathf.Lerp(1, directionalAttenuation, rangedDirectionAttenuation) * Mathf.Lerp(1, proximityAttenuation, rangedDistanceAttenuation);

                if (d > score)
                {
                    target = unit;
                    score = d;
                }
            }

            return target != null;
        }

        public void RangedAttack()
        {
            if (currentHealth <= 0)
                return;

            if (CurrentAction != Action.Idle)
                return;

            target = null;
            attackGround = false;
            CurrentAction = Action.Shoot;
            Animator.SetBool("running", false);
            Animator.SetTrigger("ranged");
            arrowObject.SetActive(true);
        }

        public void RangedAttack(ITarget target)
        {
            if (currentHealth <= 0)
                return;

            if (CurrentAction != Action.Idle)
                return;

            this.target = target;
            attackGround = false;
            CurrentAction = Action.Shoot;
            Animator.SetBool("running", false);
            Animator.SetTrigger("ranged");
            arrowObject.SetActive(true);
        }

        public void RangedAttack(Vector3 target)
        {
            if (currentHealth <= 0)
                return;

            if (CurrentAction != Action.Idle)
                return;

            this.target = null;
            rangedTarget = target;
            attackGround = true;
            CurrentAction = Action.Shoot;
            Animator.SetBool("running", false);
            Animator.SetTrigger("ranged");
            arrowObject.SetActive(true);
        }

        public void MeleeHit(ITarget target, Vector3 collisionPoint, float damage, float knockback, float knockbackDirectionAtten, float knockbackDistanceAtten, float recoilSpeed)
        {
            if (!MeleeDamageActive)
                return;

            if (target is Unit && (target as Unit).currentHealth <= 0)
                return;

            if (meleeHits.Contains(target.Parent()))
                return;
            
            meleeHits.Add(target.Parent());

            if (target.Faction() == Faction())
                return;

            if (target.RecoilsFrom(Mode, out float recoilSpeedModifier))
            {
                Recoil(collisionPoint, recoilSpeed * recoilSpeedModifier);
                return;
            }
            Vector3 strikeOrigin = Center();// transform.TransformPoint(meleeStrikeOrigin);
            Vector3 strikeDirection = transform.forward;// transform.TransformDirection(meleeStrikeDirection.normalized);

            Vector3 strikeTarget = target.Center();
            Vector3 delta = strikeTarget - strikeOrigin;

            float directionalAttenuation = Mathf.Max(0, Vector3.Dot(strikeDirection, delta.normalized));
            float proximityAttenuation = Mathf.Clamp(1f / Vector3.SqrMagnitude(delta), 0, 1);

            target.Damage(damage, delta.normalized * knockback * Mathf.Lerp(1, directionalAttenuation, knockbackDirectionAtten) * Mathf.Lerp(1, proximityAttenuation, knockbackDistanceAtten));
        }

        public void Recoil(Vector3 collisionPoint, float speed)
        {
            Animator.SetFloat("recoilSpeed", speed);

            meleeHitParticles.transform.position = collisionPoint;
            meleeHitParticles.Emit(10);
            CurrentAction = Action.Recoil;
            Animator.SetTrigger("recoil");
            Animator.SetBool("running", false);
        }

        public void Shoot()
        {
            if (CurrentAction == Action.Shoot)
                CurrentAction = Action.Idle;

            Vector3 rangedOrigin = arrowBone.TransformPoint(rangedAttackOrigin);
            Vector3 velocity;

            Vector3 targetPoint = target == null ? rangedTarget : target.RangedTarget();

            if (Random.value < missedShotFrequency)
                targetPoint += Random.insideUnitSphere * missedShotDeviationRange;

            if (target != null && Ballistics.SolveArcPitch(rangedOrigin, targetPoint, 45, out velocity))
            {
                float mag = velocity.magnitude;
                if (mag <= rangedAttackMaximumVelocity)
                {
                    if (mag < rangedAttackMinimumVelocity && Ballistics.SolveArcVector(rangedOrigin, rangedAttackMinimumVelocity, targetPoint, target.Velocity(), -Physics.gravity.y, out Vector3 s0, out Vector3 s1) > 0)
                    {
                        Projectile projectile = Instantiate(rangedAttackProjectilePrefab, rangedOrigin, Quaternion.LookRotation(s0));
                        projectile.IgnoreCollision(gameObject);
                        projectile.faction = Faction();
                        projectile.target = target;
                        projectile.Rigidbody.velocity = s0;
                    }
                    else
                    {
                        //transform.rotation = rotation;
                        Projectile projectile = Instantiate(rangedAttackProjectilePrefab, rangedOrigin, Quaternion.LookRotation(velocity));
                        projectile.IgnoreCollision(gameObject);
                        projectile.faction = Faction();
                        projectile.target = target;
                        projectile.Rigidbody.velocity = velocity;
                    }
                }
            }

            else if (attackGround && Ballistics.SolveArcPitch(rangedOrigin, targetPoint, 45, out velocity))
            {
                float mag = velocity.magnitude;
                if (mag <= rangedAttackMaximumVelocity)
                {
                    if (mag < rangedAttackMinimumVelocity && Ballistics.SolveArcVector(rangedOrigin, rangedAttackMinimumVelocity, targetPoint, -Physics.gravity.y, out Vector3 s0, out Vector3 s1) > 0)
                    {
                        Projectile projectile = Instantiate(rangedAttackProjectilePrefab, rangedOrigin, Quaternion.LookRotation(s0));
                        projectile.IgnoreCollision(gameObject);
                        projectile.faction = Faction();
                        projectile.Rigidbody.velocity = s0;
                    }
                    else
                    {
                        //transform.rotation = rotation;
                        Projectile projectile = Instantiate(rangedAttackProjectilePrefab, rangedOrigin, Quaternion.LookRotation(velocity));
                        projectile.IgnoreCollision(gameObject);
                        projectile.faction = Faction();
                        projectile.Rigidbody.velocity = velocity;
                    }
                }
            }

            else
            {
                Debug.Log("shootin free");
                float pitch = 30;
                float speed = rangedAttackMinimumVelocity;
                float yaw = 90 - Mathf.Atan2(transform.forward.z, transform.forward.x) * Mathf.Rad2Deg;

                velocity = Quaternion.AngleAxis(yaw, Vector3.up) * Quaternion.AngleAxis(-pitch, Vector3.right) * Vector3.forward * speed;

                Projectile projectile = Instantiate(rangedAttackProjectilePrefab, rangedOrigin, Quaternion.LookRotation(velocity));
                projectile.IgnoreCollision(gameObject);
                projectile.faction = Faction();
                projectile.Rigidbody.velocity = velocity;
            }

            target = null;
            attackGround = false;
            arrowObject.SetActive(false);
        }

        public void DropHeldItems()
        {
            ItemType mhType = HeldType;
            ItemType ohType = OffHand.Item(0);

            string mhName = mhType?.boneName;
            string ohName = ohType?.boneName;

            int mhQuantity = HeldQuantity;
            int ohQuantity = OffHand.Quantity(0);


            GameObject mainHand = null;
            GameObject offHand = null;

            foreach (GameObject bone in toolBones)
            {
                if (mhName != null && bone.gameObject.name == mhName)
                    mainHand = bone;

                else if (ohName != null && bone.gameObject.name == ohName)
                    offHand = bone;
            }


            if (mainHand != null)
            {
                Item i = Instantiate(mhType.prefab, mainHand.transform.position, mainHand.transform.rotation);
                i.type = mhType;
                i.quantity = mhQuantity;

                //Item i = mainHand.gameObject.AddComponent<Item>();
                //i.type = mhType;
                //i.quantity = mhQuantity;
                //mainHand.gameObject.layer = LayerMask.NameToLayer("Items");
                //mainHand.transform.parent = null;

                Inventory.Remove(heldIndex, out _, out _);
            }

            if (offHand != null)
            {
                Item i = Instantiate(ohType.prefab, offHand.transform.position, offHand.transform.rotation);
                i.type = ohType;
                i.quantity = ohQuantity;

                //Item i = offHand.gameObject.AddComponent<Item>();
                //i.type = ohType;
                //i.quantity = ohQuantity;
                //offHand.gameObject.layer = LayerMask.NameToLayer("Items");
                //offHand.transform.parent = null;
                OffHand.Clear();
            }

            if (pickupItem != null)
            {
                EmitItem(pickupItem.type, pickupItem.quantity, out _);
                Destroy(pickupItem.gameObject);
            }
        }

        public void DropInventory()
        {
            for (int i = 0; i < Inventory.Length; i++)
                EmitItem(Inventory.Item(i), Inventory.Quantity(i), out _);

            Inventory.Clear();
        }

        public override void Kill()
        {
            base.Kill();
            DropHeldItems();
            DropInventory();
        }

        private void FixedUpdate()
        {
            if (Mode != ItemMode.Sword)
                blocking = false;

            Animator.SetBool("blocking", blocking);

            if (CurrentHealth() <= 0)
            {
                CurrentAction = Action.Idle;
            }

            if (CurrentAction == Action.Melee)
            {
                if (target != null)
                {
                    Vector2 delta = (target.Center() - transform.position).xz();
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.AngleAxis(90 - Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg, Vector3.up), Time.deltaTime * rotationSpeed);
                }
            }
            else
            {
                if (meleeHits == null)
                    meleeHits = new List<ITarget>();
                else
                    meleeHits.Clear();
            }

            if (CurrentAction == Action.Shoot)
            {
                if (target != null)
                {
                    Vector2 delta = (target.Center() - transform.position).xz();
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.AngleAxis(90 - Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg, Vector3.up), Time.deltaTime * rotationSpeed);
                }
                else if (attackGround)
                {
                    Vector2 delta = (rangedTarget - transform.position).xz();
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.AngleAxis(90 - Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg, Vector3.up), Time.deltaTime * rotationSpeed);
                }
            }
            else
            {
                arrowObject.SetActive(false);
            }

            if (pickupItem != null)
            {
                if (CurrentAction == Action.PickUp)
                {
                    AnimatorStateInfo info = Animator.GetCurrentAnimatorStateInfo(0);
                    //float t = info.IsName("Pickup") ? info.normalizedTime : 0;

                    pickupTimer += Time.deltaTime;
                    float t = pickupTimer / pickupTime;

                    pickupItem.transform.position = Vector3.Lerp(pickupItem.transform.position, hand_left.position, pickupPositionFunction.Evaluate(t));
                    pickupItem.transform.localScale = Vector3.one * pickupScaleFunction.Evaluate(t);
                }
                else
                {
                    pickupTimer = 0;
                    Inventory.Add(pickupItem.type, pickupItem.quantity);
                    Destroy(pickupItem.gameObject);
                    pickupItem = null;
                }
            }
            else
            {
                pickupTimer = 0;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(.2f, .4f, 1f, .5f);
            Gizmos.DrawSphere(transform.TransformPoint(pickupOffset), pickupRadius);
        }
    }
}