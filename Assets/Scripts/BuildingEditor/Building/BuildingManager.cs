using System.Collections.Generic;
using UnityEngine;

public class BuildingManager {

    public static List< BuildBookVO > BookList = new List< BuildBookVO >();
    public static Dictionary< int, BuildBookVO > objBook = new Dictionary< int, BuildBookVO >();

    public static List< BuildingVO > list = new List< BuildingVO >();

    public static void Init() {
        BuildingCatalog catalog = Resources.Load< BuildingCatalog >( "BuildingCatalog" );

        foreach( BuildBookVO book in catalog.Buildings ) {
            SetBookData( book );
        }

        Debug.Log( $"BuildingManager initialized with {BookList.Count} buildings" );
    }

    public static void SetBookData( BuildBookVO book ) {
        BookList.Add( book );
        objBook[ book.kindNum ] = book;
    }

    // kindNum으로 빌딩 데이터 찾기
    public static BuildBookVO GetBuilding( int kindNum ) {
        if( objBook.ContainsKey( kindNum ) ) {
            return objBook[ kindNum ];
        }
        return null;
    }

    // 모든 빌딩 데이터 가져오기
    public static List< BuildBookVO > GetAllBuildings() {
        return new List< BuildBookVO >( BookList );
    }

    // 빌딩 개수
    public static int Count => BookList.Count;

}