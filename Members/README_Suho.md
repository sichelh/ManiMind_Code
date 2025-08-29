
## ManiMind_Suho 개발문서

---

## 주요개발목록

  - [1. 스킬시스템](#1-스킬-시스템)
  - [2. 스킬연출](#2-스킬연출)
  - [3. VFX](#3-VFX시스템)
  - [4. SFX](#4-SFX시스템)
  - [5. 적대치/적스킬사용로직](#5-적대치/적스킬사용로직)

---

# 1. 스킬 시스템

#### 설계목적
- 스킬의 내부구조를 모르는 사람도 스킬데이터를 만들 수 있게 ScriptableObject의 필드를 지정.
- 다양한 스킬을 구현할 수 있도록 범용적인 스킬시스템 구현.
- 다른 팀원의 코드 구조에 맞게 스킬효과를 구현.
  
<hr>

#### 구현방식
- BaseSkillController라는 제네릭클래스를 만들어 함수 재사용 및 책임분산.
- 스킬데이터를 ActiveSkillSO라는 ScriptableObject로 저장.
  <img width="594" height="463" alt="image" src="https://github.com/user-attachments/assets/e7479a30-6df7-4b1f-9f8c-fd20a531b4e3" />
- 범위스킬의 경우 TargetSelect라는 클래스에서 유효유닛을 찾아오도록 만들어 책임분산.

#### 근거리스킬과 원거리스킬 구현
- 근거리 스킬의 경우 ActiveSkillSO의 AnimationClip을 Play하며 애니메이션 트리거에 맞춰서 적유닛에 스킬효과를 부여.
- 원거리 스킬의 경우에는 애니메이션 트리거때 투사체가 발사. 해당 투사체는 ObjectPool을 사용하여 재사용가능.
- 투사체에 ActiveSkillSO데이터를 참조하여 투사체가 적과 충돌 시에 해당 스킬효과를 ACtiveSkillSO데이터를 참고하여 적용.
---
### 2. 스킬연출

#### 설계목적
- 턴제게임에서 유저에게 보여줄 수 있는 타격감, 액션을 지원.
- 높은 등급의 스킬에 액션을 넣어 유저가 높은 등급의 스킬을 원하는 욕구를 만들어 동기부여.

#### 구현방식
- 타임라인을 활용하여 스킬컷씬을 구현.
- [타임라인을 활용한 스킬컷씬 구현링크](https://velog.io/@suho1213/20250722TIL)
- ![2025-08-04 11-24-07](https://github.com/user-attachments/assets/0e488092-8573-42f7-9f5e-06e5a1501d24)
![2025-08-04 11-25-12](https://github.com/user-attachments/assets/437f43a4-fb56-4745-a513-9bd5ed6fb594)
![2025-08-04 11-24-38](https://github.com/user-attachments/assets/cb0181a3-1ae5-49f3-9837-4b95fd7b9f66)
![2025-08-04 11-25-52](https://github.com/user-attachments/assets/ef41b2ed-3563-4f04-b310-5da32a764f79)

- 카메라 시네머신을 활용하여 역동적인 스킬사용효과를 연출.
 - ![2025-08-05 20-49-21](https://github.com/user-attachments/assets/8434a287-7b6f-4b11-9c11-53372965593e)
 - Timeline의 Signal Receiver를 활용하여 카메라흔들림, 스킬사용효과 등의 시점을 제어.

#### 애니메이션 리깅
- 타임라인에서 사용되는 애니메이션은 [mixamo](https://www.mixamo.com/#/)에서 스켈레톤 애니메이션으로 받아와 이를 유니티의 애니메이션 리깅(Animation Rigging) 패키지를 통해 수정하였다.
- [애니메이션 리깅을 활용한 애니메이션 수정 포스트 링크](https://velog.io/@suho1213/20250723TIL)
- 간단한 휴머노이드 애니메이션을 수정하여 재사용.




---
### 3. VFX시스템

#### 설계목적
- 다양한 VFX를 리소스낭비 없이 쉽게 사용할 수 있도록 구현.
- ObjectPool을 통하여 재사용가능한 형태로 구현.
- 내부구조를 모르더라도 VFX를 쉽게 적용가능한 형태로 구현.

#### 구현방식
- VFXData라는 래퍼클래스를 정의하여 사용.
  - VFXData는 ObjectPool이 적용된 VFX Object를 초기화하는데 필요한 데이터를 저장.

---

### 4. SFX시스템

#### 설계목적
- 어드레서블을 활용해 성능 최적화.
- ObjectPool을 활용하여 성능 최적화.
- AudioManager를 통해 손쉽게 SFX를 출력할 수 있게 함.

#### 구현방식
- 어드레서블을 적용하여 특정 씬에서 필요한 사운드만 Lable을 통해 메모리에 할당.
- SFX를 출력하는 AudioSource Object는 Object Pool을 활용하여 재사용.
- 같은 효과음 중복으로 인한 사운드 증폭효과를 방지하기 위하여, 동시에 같은 키값을 가진 SFX가 4개 이상 존재할 수 없도록 로직적용.

----

### 5. 적대치/스킬 사용 로직
- WeightedSelector라는 유틸리티 클래스를 생성하여 동일한 로직을 두 시스템에 적용.
- 유효한 Entity인지 확인하는 로직과 Entity의 확률 값을 가져오는 로직을 대리자를 통해 전달.

