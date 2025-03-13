### Unity Isometric 2D System (Job System Supported)

![](https://github.com/MyNameIsDabin/UnityIsometric2D/blob/master/Guide.png)

Unity에서 2D 환경에서의 isometric뷰에서의 오브젝트 정렬 문제를 해결하기 위해 다양한 전략이 있지만 그 중에서도 Transparency Sort Axis를 이용한 정렬은 제약이 많아 직접 구현해본 Unity에서 사용 가능한 커스텀 정렬 구현입니다. 

유니티의 Job System을 이용한 성능 최적화를 적용하고 에디터에서 실시간 정렬 지원과 더불어 Topology Sort에 대한 디버깅이 가능하도록 하는 목적에서 개발하였습니다.

(원한다면 인스펙터에서 Sorter Type을 언제든 바꿔 Job System을 사용하지 않을 수 있습니다.)

### 사용 방법
![](https://github.com/MyNameIsDabin/UnityIsometric2D/blob/master/Guide1.png)

- IsometricSortingGroup
- IsometricSpriteRenderer

위 컴포넌트를 정렬하기를 원하는 Sprite Renderer 혹은 SortingGroup 컴포넌트와 함께 부착하면 됩니다. 사용 예는 프로젝트의 SampleScene2.unity를 참고하시기 바랍니다.


### 주의 사항
현재 구현된 방식의 부작용으로 바닥 면이 충돌하는 경우에 대한 정렬이 자연스럽지 않을 수 있습니다. (그렇게 배치 되지 않도록 하는 것을 권장 합니다.)
