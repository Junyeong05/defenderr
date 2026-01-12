using UnityEngine;

/*
public class BuildingFactory {
    private static BuildingFactory instance;

    public static BuildingFactory Instance {
        get {
            if( instance == null ) {
                GameObject go = new GambeObject( "BuildingFactory" );
                instance = go.AddComponent< BuildingFactory >();
            }
            return instance;
        }
    }

    private Dictionary< string, GameObject[] > buildingPool = new Dictionary< string, GameObject[] >();

    public static GetBuilding( string type ) {

        if( !buildingPool.ContainsKey( type ) ) {
            buildingPool[ type ] = new Queue< GameObject >();
        }

        GameObject building;

        if( buildingPool[ type ].Count > 0 ) {
            building = buildingPool[ type ].pop();
        } else {
            building = new GameObject( "Building" );
            building.AddComponenet< "Building" >();
        }

        return building;
    }

    public static AddToRecycleList( string type, GameObject building ) {
        if( buildingPool[ type ].Count > 0 ) {
            buildingPool[ type ].add( building );
        }
        buildingPool.Add( type, building );
    }
}
*/