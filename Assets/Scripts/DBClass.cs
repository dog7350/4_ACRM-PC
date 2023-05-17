public class DBClass
{
    public class userinfo // �α���, ����, ���� �÷���
    {
        public string id;
        public string pw;
        public int money;
        public int cash;
        public int admin;
    }

    public class gameinfo // �α���, ����(����)
    {
        public string id;
        public string usecar;
        public string usegun;
        public string invitf;
        public string difficult;
        public string room;
        public string connect;
    }

    public class inviinfo // �κ�, ��ð���
    {
        public string id;
        public string invitation;
        public string room;
        public string inviDate;
    }
    public class inviuser
    {
        public string id;
        public string usecar;
        public string usegun;
    }

    public class carinfo // �ε�
    {
        public int cnum;
        public string cname;
        public string content;
        public float speed;
        public float hp;
        public float def;
        public int price;
        public string cash;
    }

    public class guninfo // �ε�
    {
        public int gnum;
        public string gname;
        public string content;
        public float atk;
        public float rpm;
        public int ammo;
        public int price;
        public string cash;
    }

    public class track // �ε�
    {
        public int tnum;
        public string tname;
        public string content;
    }

    public class usecar // �ε�, ����(����)
    {
        public string id;
        public int cnum;
    }

    public class usegun // �ε�, ����(����)
    {
        public string id;
        public int gnum;
    }

    public class history
    {
        public string id;
        public string usecar;
        public string usegun;
        public string matchDate;
        public string result;
        public string resultcar;
        public string resultgun;
    }

    public class dbEnb
    {
        public string ename;
        public string evalue;
    }
}
