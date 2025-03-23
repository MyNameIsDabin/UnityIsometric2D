### Unity Isometric 2D System (Job System Supported)

![](https://github.com/MyNameIsDabin/UnityIsometric2D/blob/master/Guides/Overview.png)

Unity 에서 사용 가능한 2.5D (2D) isometric 정렬 플러그인 입니다.

### 시스템 요구 사항
- Unity: 2021.3 or later

### 사용 방법

![](https://github.com/MyNameIsDabin/UnityIsometric2D/blob/master/Guides/GitGuide.png)

URL : `https://github.com/MyNameIsDabin/UnityIsometric2D.git?path=/Assets/Isometric2D`

유니티 Package Manager에서 + 버튼을 눌러 'Add package from git URL..'을 선택한 후, 위 URL을 붙여넣기 해서 플러그인을 임포트 불러옵니다.

사용 예는 샘플 프로젝트를 받아 Sample.unity 씬을 참고하시기 바랍니다.

혹은 git 저장소를 통째로 clone으로 받아 필요한 스크립트를 포함해도 됩니다.

### 기능

#### 시각적 디버깅과 Job System 지원

![](https://github.com/MyNameIsDabin/UnityIsometric2D/blob/master/Guides/Feature1.png)

에디터에서 정렬 판정에 대한 시각적인 디버그를 지원하며 Job System을 활용한 고성능 정렬 판정을 지원합니다. (원한다면 싱글 스레드로 동작하는 정렬 방식을 사용할 수 있습니다. Job System을 사용하지 않는다면 좀 더 낮은 버전의 유니티에서 동작할 수 있습니다.)

#### Sorting Group, Sprite Renderer 소트 오더 정렬 지원

![](https://github.com/MyNameIsDabin/UnityIsometric2D/blob/master/Guides/Feature2.png)

IsometricSortingGroup와 IsometricSpriteRenderer 컴포넌트를 정렬하기를 원하는 Sprite Renderer 혹은 SortingGroup 컴포넌트와 함께 부착하여 사용하면 됩니다.

이 두 컴포넌트는 IsometricObject 컴포넌트를 의존하도록 되어있습니다. 이 컴포넌트는 실제 계산된 Order 정보를 가지기 때문에 원한다면 스프라이트 sortOrder가 아닌 다른 정렬 기준을 직접 커스텀해서 사용할 수도 있습니다. `IsometricOrderBinder` 클래스를 상속해서 구현하면 됩니다.

Topology 정렬 방식의 한계로 인해 객체가 많아지면 금방 퍼포먼스가 급감합니다. 위 컴포넌트들은 보여지는 객체만 정렬 목록에 포함시키는 culling 옵션이 지원됩니다.

### LossyScale에 따른 Isometric 영역 대응

![](https://github.com/MyNameIsDabin/UnityIsometric2D/blob/master/Guides/Feature3.gif)

배치된 오브젝트를 뒤집거나 스케일을 변경하고 싶을 수 있습니다. 혹은 부모 개체의 스케일이 변할 수도 있습니다. 이 때마다 IsometricObject의 크기를 바꾸어야 하는 것은 상당히 번거롭기 때문에 기본적으로 lossyScale을 기반으로 충돌 영역이 자동으로 재설정 됩니다.

### 라이선스
[CC0](https://creativecommons.org/publicdomain/zero/1.0/)
