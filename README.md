### Unity Isometric 2D System (Job System Supported)

![](https://github.com/MyNameIsDabin/UnityIsometric2D/blob/master/Guide.png)

Unity에서 2D 환경에서의 isometric뷰에서의 오브젝트 정렬 문제를 해결하기 위한 다양한 방법이 있지만 그 중에서도 기본 Unity에서 제공하는 Transparency Sort Axis를 이용한 정렬은 제약이 많아 직접 구현해본 Isometric 2D 정렬입니다.

유니티의 Job System을 이용한 성능 최적화를 적용하고 에디터에서 실시간 정렬 지원과 더불어 Topology Sort에 대한 디버깅이 가능하도록 하는 목적에서 개발하였습니다.

(원한다면 인스펙터에서 Sorter Type을 언제든 바꿔 Job System을 사용하지 않을 수 있습니다.)

### 사용 방법
![](https://github.com/MyNameIsDabin/UnityIsometric2D/blob/master/Guide1.png)

- IsometricSortingGroup
- IsometricSpriteRenderer

위 컴포넌트를 정렬하기를 원하는 Sprite Renderer 혹은 SortingGroup 컴포넌트와 함께 부착하면 됩니다. 사용 예는 프로젝트의 SampleScene2.unity를 참고하시기 바랍니다.

이 두 컴포넌트는 IsometricObject를 의존하도록 되어있습니다. 이 컴포넌트는 실제 계산된 Order 정보를 가지기 때문에 원한다면 스프라이트 sortOrder가 아닌 다른 정렬 기준을 사용할 수도 있습니다. `IsometricOrderBinder.cs`를 참고해보세요.