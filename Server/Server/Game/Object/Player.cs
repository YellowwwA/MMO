using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Server.DB;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public class Player : GameObject
    {
        public int PlayerDbId { get; set; }
        public ClientSession Session { get; set; }
        public Inventory Inven { get; private set; } = new Inventory();

        public int WeaponDamage { get; private set; }
        public int ArmorDefence { get; private set; }

        public override int TotalAttack { get { return Stat.Attack + WeaponDamage; } }
        public override int TotalDefence { get { return ArmorDefence; } }

        public Player()
        {
            ObjectType = GameObjectType.Player;
            Speed = 20.0f;
        }

        public override void OnDamaged(GameObject attacker, int damage)
        {
            base.OnDamaged(attacker, damage);
        }

        public override void OnDead(GameObject attacker)
        {
            base.OnDead(attacker);
        }

        public void OnLeaveGame()
        {
            //DB 연동
            //한명이 전부 일을 하는게아니라 transaction한테 일감을 던져준 것
            DbTransaction.SavePlayerStatus_Step1(this, Room);
        }

        public void HandleEquipItem(C_EquipItem equipPacket)
        {
            Item item = Inven.Get(equipPacket.ItemDbId);
            if (item == null)
                return;

            if (item.ItemType == ItemType.Consumable) //소비아이템은 착용불가
                return;

            //착용 요청이라면, 겹치는 부위 해제
            if (equipPacket.Equipped)
            {
                Item unequipItem = null;

                if (item.ItemType == ItemType.Weapon)
                { //무기는 하나만 착용가능
                    unequipItem = Inven.Find(
                        i => i.Equipped && i.ItemType == ItemType.Weapon); //이미 무기를 착용하고있다면 (벗어야되는 무기 찾아주기)
                }
                else if (item.ItemType == ItemType.Armor)
                { //한 부위에는 하나의 방어구만 착용 가능 ex.헬멧&신발은 가능 but 헬멧2개는 불가능
                    ArmorType armorType = ((Armor)item).ArmorType;
                    unequipItem = Inven.Find(
                        i => i.Equipped && i.ItemType == ItemType.Armor && ((Armor)i).ArmorType == armorType);
                }

                if (unequipItem != null) //벗어야되는 무기가 있는경우
                {
                    // 메모리 선 적용
                    unequipItem.Equipped = false;

                    // DB에 Noti
                    DbTransaction.EquipItemNoti(this, unequipItem);

                    //클라에 통보
                    S_EquipItem equipOkItem = new S_EquipItem();
                    equipOkItem.ItemDbId = unequipItem.ItemDbId;
                    equipOkItem.Equipped = unequipItem.Equipped;
                    Session.Send(equipOkItem);
                }
            }

            {
                // 메모리 선 적용
                item.Equipped = equipPacket.Equipped;

                // DB에 Noti
                DbTransaction.EquipItemNoti(this, item);

                //클라에 통보
                S_EquipItem equipOkItem = new S_EquipItem();
                equipOkItem.ItemDbId = equipPacket.ItemDbId;
                equipOkItem.Equipped = equipPacket.Equipped;
                Session.Send(equipOkItem);
            }

            RefreshAdditionalStat();
        }

        public void RefreshAdditionalStat() //아이템착용에 의한 추가 스탯 갱신
        {
            WeaponDamage = 0;
            ArmorDefence = 0;

            foreach(Item item in Inven.Items.Values)
            {
                if (item.Equipped == false)
                    continue;

                switch(item.ItemType)
                {
                    case ItemType.Weapon:
                        WeaponDamage += ((Weapon)item).Damage;
                        break;
                    case ItemType.Armor:
                        ArmorDefence += ((Armor)item).Defence;
                        break;
                }
            }
        }
    }
}
