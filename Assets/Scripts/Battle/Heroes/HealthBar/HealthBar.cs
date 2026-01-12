using UnityEngine;

public class HealthBar : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // void Start()
    // {
    //     LoadTexture();
    //     CallRenderer();
    //     AddSprite();
    // }

    public void Draw( bool IsAlly ) {
        if( IsAlly == true ) {
            LoadPlayerTexture();
        } else if( IsAlly == false ) {
            LoadEnemyTexture();
        }
        CallRenderer();
        AddSprite();
    }

    [SerializeField]
    GameObject BarBg;
    [SerializeField]
    GameObject BarSp;

    public float BarWidthScale = 20.0f;
    public float BarHeightScale = 5.0f;
    
    private Sprite BarBgSprite;
    private Sprite BarSpSprite;

    private SpriteRenderer BgRenderer;
    private SpriteRenderer SpRenderer;

    private void LoadPlayerTexture() {
        BarBgSprite = TextureManager.GetSprite( "atlases/Battle", "UnitBar_red_center" );
        BarSpSprite = TextureManager.GetSprite( "atlases/Battle", "UnitBar_green_center" );
    }

    private void LoadEnemyTexture() {
        BarBgSprite = TextureManager.GetSprite( "atlases/Battle", "UnitBar_red_center" );
        BarSpSprite = TextureManager.GetSprite( "atlases/Battle", "UnitBar_blue_center" );
    }

    private void CallRenderer() {
        BgRenderer = BarBg.GetComponent< SpriteRenderer >();
        SpRenderer = BarSp.GetComponent< SpriteRenderer >();

        BgRenderer.sortingOrder = 1;
        SpRenderer.sortingOrder = 2;
    }

    private void AddSprite() {
        BgRenderer.sprite = BarBgSprite;
        SpRenderer.sprite = BarSpSprite;

        ChangeSp( 1 );
        ChangeBg( 1 );
    }

    public void ChangeHp( float currentHp ) {
        if( currentHp <= 0 ) {
            ChangeSp( 0 );
            return;
        };

        ChangeSp( currentHp );

    }

    private void ChangeSp( float scale ) {
        BarSp.transform.localScale = new Vector3( scale * BarWidthScale, BarHeightScale, 0 );
    }

    private void ChangeBg( float scale ) {
        BarBg.transform.localScale = new Vector3( scale * BarWidthScale, BarHeightScale, 0 );
    }

}
