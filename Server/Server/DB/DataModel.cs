using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Server.DB
{
    [Table("Account")]
    public class AccountDb
    {
        public int AccountDbId { get; set; }
        public string AccountName { get; set; } 
        public ICollection<PlayerDb> Players { get; set; }
    }

    [Table("Player")]
    public class PlayerDb
    {
        public int PlayerDbId { get; set; }
        public string PlayerName { get; set; }

        [ForeignKey("Account")]
        public int AccountDbId { get; set; }
        public AccountDb Account { get; set; }

        public ICollection<ItemDb> Items { get; set; } //내가 갖고있는 아이템 목록 (플레이어 하나당 아이템 여러개 보유)

        public int Level { get; set; }
        public int Hp { get; set; }
        public int MaxHp { get; set; }
        public int Attack { get; set; }
        public float Speed { get; set; }
        public int TotalExp { get; set; }

    }

    [Table("Item")]
    public class ItemDb
    {
        public int ItemDbId { get; set; } //db에서 가져온 아이템 아이디
        public int TemplateId { get; set; } //데이터시트에서 어떤 아이디인지 구분 (내가 지정)
        public int Count { get; set; }
        public int Slot { get; set; } //인벤토리 내 아이템들의 슬롯 위치 저장, 인덱스 번호를 1~10은 현재 보유아이템 11~20은 창고에 넣은 아이템 등 이렇게 분류도 가능
        public bool Equipped { get; set; } // 장착여부

        [ForeignKey("Owner")]
        public int? OwnerDbId { get; set; } //보유자 없이 땅바닥에 버린 아이템일경우를 대비해 nullable타입으로 ?붙임
        public PlayerDb Owner { get; set; } //아이템 보유자

    }
}
