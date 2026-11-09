# M10-13 사용자 플레이 게이트 보고서

상태: 준비 중 — 사용자 평가와 명시적 수락 대기. 기본 커밋: bdaf1dea39ff2e31ea72ed078cee9a0ea8c6eab6. 브랜치: feat/m10-13-user-play-gate. 사용자 의견과 승인 기록은 아직 없습니다.

## 장면별 평가표

각 칸은 사용자가 직접 평가한 뒤 기록합니다. 결정: PASS, PASS_WITH_APPROVED_EXCEPTIONS, FAIL, NOT_TESTED, AWAITING_USER_REVIEW.

| 장면 | 화면 / 인물 | 서사 호흡 | 음악 / 효과음 | 조작 | 문제 / 사용자 의견 | 결정 |
|---|---|---|---|---|---|---|
| S001 가족의 아침 | 주방, 다섯 사람과 보통 로프, 크기와 겹침 | 일상과 대화의 자연스러움 | 아침 BGM 시작 | 대사 진행 / 가독성 | 미기록 | AWAITING_USER_REVIEW |
| S002 미묘한 불편함 | 두 해변 사진, 구도, 반응 | 불편함의 점진적 전달 | 아침 음악 연속, 의자 소리 | 대사 진행 | 미기록 | AWAITING_USER_REVIEW |
| S003 로프와 상담사 | 내면 독백, 발표자, 정보 패널 | 상담사 공개 타이밍 | 웃음 반복, 음악 전환 | 패널 가독성 | 미기록 | AWAITING_USER_REVIEW |
| S004 수납실 | 헌 신발, 새 신발, 가방, 표정 | 침묵과 감정 전환 | 침묵의 효과 | 수동 대기와 해제 | 미기록 | AWAITING_USER_REVIEW |
| S005 작별 | 가족 전원, 배치, 진희 | 작별과 감정의 해소 | 작별 BGM 시작 | 대사 진행 | 미기록 | AWAITING_USER_REVIEW |
| S006 흰 문과 현실 | 국소 흰 문, 흔들림 없음, 두 로프의 구분 | 현실 공개의 효과 | BGM 정지 | 대사 진행 | 미기록 | AWAITING_USER_REVIEW |
| S007 진료 | 의사, 정보 패널, 의료 도표 | 설명의 이해와 호흡 | 대사 중 음량 | 한글 가독성 | 미기록 | AWAITING_USER_REVIEW |
| S008 프로필과 결말 | 세 카드, 초상, 장비, 엔딩 타이틀 | 첫 검토 생각, 하나 선택, 결말 | 장비, 비, 물방울, 버스 브레이크 | 프로필 탐색 / 선택 | 미기록 | AWAITING_USER_REVIEW |

## 실제 입력 체크리스트

- [ ] 스페이스와 마우스 클릭: 한 번에 한 문단, 타자 효과 빠르게 표시와 다음 문단 구분, 연타/길게 누르기.
- [ ] Escape: 설정, 저장/불러오기, 기록에서 닫기/뒤로; 리플레이 취소; 모달 뒤 대사 정지.
- [ ] 자동 시작/정지, 읽은 문장만 스킵, 전체 스킵과 수동 대기 제한.
- [ ] 숨김 복원 첫 입력, 백로그 열기/닫기.
- [ ] 수동/퀵/자동 저장과 불러오기, 덮어쓰기 확인/취소, 정확한 문단과 다음 문단.
- [ ] S004 수동 대기, S006 흰 문, S008 세 프로필과 첫 검토 생각 및 대화 복귀.

## 콘텐츠 결정 및 권리

기계 로프는 임시 마커이며 별도 기계형 몸체와 default/alert 표정이 필요합니다. 결정: WAITING_FOR_USER_ASSET. 보통 강아지 rope_memory와 구별합니다. 다섯 무음 대체는 sfx_plate_counter (S001 조리대), sfx_child_entry_steps (S001 입장), sfx_sink_washing (S005 설거지), sfx_slipper_toe_tap (S006 두 번), sfx_raindrop_awning (S008 차양 물방울)입니다. 최종 소리나 무음의 수락은 사용자 판단을 기다립니다. 긴 빗소리를 짧은 물방울 대신 사용하지 않습니다.

19개 캐릭터별 표정 키의 실제 몸체 파일, 사용 장면 및 없는 헤드 목록은 [M10-08 상세 목록](M10_08_POST_MERGE_HARDENING_REPORT.md)의 Expression-art delivery register를 참고합니다. 현재 별도 headSprite가 없을 때 몸체 이미지를 사용합니다. 새 이미지의 수락은 기록되지 않았습니다.

사용자 제공 이미지와 음악/효과음은 개별 제작자 및 상업적 이용 증거가 불완전합니다. Mixkit 표기 빗소리는 정확한 다운로드 출처 증거가 없습니다. NotoSansKR 파일 이름 Regular와 기록된 PostScript 이름 NotoSansCJKkr-Thin이 일치하지 않아 식별 및 라이선스 증거를 확인해야 합니다. 예술적 수락과 배포 권리는 별도이며 배포 허가는 주장하지 않습니다. 개별 출처 및 해시 장부는 M10-08 보고서에 있습니다.

## 기술 위험과 이슈

M10-12: 빌드 경고 486개, 종료 시 원인 미확인 ComputeBuffer 경고, 장시간 메모리 프로파일 미실시, 2560×1440 환경 미지원, 일부 저장 실패 주입 미실시. 사용자에게 보이는 재현 가능한 증상이 발견되면 조사합니다.

| 이슈 ID | 장면/대사 | 기대 / 실제 | 재현 | 심각도 P0/P1/P2 | 증거 | 제안 / 실제 수정 | 재검증 | 사용자 수락 |
|---|---|---|---|---|---|---|---|---|
| 없음 | — | 아직 사용자 피드백 없음 | — | — | — | — | — | AWAITING_USER_REVIEW |

빌드 식별자, 실제 저장 경로, 단독 실행 및 저장/재시작 결과는 검증 후 기록합니다. M10-14는 시작하지 않았습니다.

## 2026-11-09 QA 빌드 진행 및 미완료 확인

- 기준 소스: bdaf1dea39ff2e31ea72ed078cee9a0ea8c6eab6. 별도 임시 프로젝트 복사본에서 Unity 직렬화 API로 시작 노드를 PROLOGUE_S001_START로 설정하고 QA 전용 회사/제품 이름을 사용했습니다. 추적되는 원본 VN_Main과 ProjectSettings는 변경하지 않았습니다.
- Windows x64 BuildReport: Succeeded, 오류 0, 경고 486, 크기 347132612 bytes. 실행 파일 SHA-256: 48EFAB523AA684C653BD1254A6962D3410127B5C02DC1310F6F16F4810666556.
- 실행 파일: C:\Dev\project_all_time_playtest\M10_13\Build\AllTime_M10_13_PlayGate.exe. 시작 시 S001의 첫 한국어 대사와 다음 대사를 확인했습니다. 화면/소리의 주관적 수락은 미실시입니다.
- Player가 보고한 Application.persistentDataPath: C:\Users\UserK\AppData\LocalLow\ProjectAllTimeQA\AllTime_M10_13_PlayGate. 같은 루트의 MetaProgress\meta_progress.json 생성 확인. 일반 게임 데이터는 대상으로 삼지 않았습니다.
- 수동 SaveData 생성, 정확한 문단 Load 및 다음 문단, 종료/재실행 후 보존은 아직 검증되지 않았습니다. 예상 경로는 위 루트의 SaveData이지만 실제 쓰기는 확인하지 못했습니다.
- 실제 Player 입력 확인 중 컴퓨터 사용 도구의 창 활성화가 실패했고 GetCursorPos returned access denied (0x80070005). 창 재선택과 활성화 재시도도 실패했습니다. 이 때문에 직접 수동 저장 화면을 검증할 수 없었습니다. 다른 입력 주입 방식으로 우회하지 않았습니다.
- 현재 Player.log에는 D3D12 info-queue query 0x80004002가 보이며, 확인 시점까지 새 프로젝트 예외는 발견되지 않았습니다. Player 종료 후 로그는 아직 확인하지 못했습니다.
- 아직 PLAYTEST BUILD READY 상태가 아닙니다. 남은 작업: 수동 저장/로드 및 후속 문단, QA 저장 파일과 재실행 보존 확인, 배경/캐릭터/BGM 재생 확인, 종료 후 로그 확인. 사용자 평가 및 최종 PR도 대기 중입니다.
