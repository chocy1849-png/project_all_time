# M10-13 사용자 플레이 게이트 보고서

**M10-13 USER PLAY GATE PASSED — QA PROLOGUE APPROVED**

**PASS — USER-REPORTED QA PLAY GATE ACCEPTANCE** · 수락일: 2026-11-09 · 기본 커밋: bdaf1dea39ff2e31ea72ed078cee9a0ea8c6eab6 · 브랜치: feat/m10-13-user-play-gate.

The report's final note that M10-14 had to wait for PR #62 is a point-in-time prerequisite. PR #62 has since merged at `6f130c33a257d0dd83a0b0657528d900de34e73c`; documentation closure is tracked in the [M10-14 report](M10_14_PROLOGUE_MILESTONE_CLOSURE_REPORT.md).

## 사용자 직접 평가 및 수락 범위

사용자는 현재 대화의 M10-13 완료 요청에서 다음 결과를 직접 보고했습니다: “I personally played the entire prologue from beginning to end. I tested saving, loading, and restarting the game. Everything worked without problems. I also checked the sound and other aspects, and found no issues.” 사용자는 현재 QA 프롤로그 경험을 승인했습니다.

S001–S008 전체를 직접 플레이했고 수동 저장/불러오기와 재실행 보존, 일반적인 실제 입력, 화면과 소리를 포함한 경험에 문제가 없었다는 **사용자 보고**입니다. 이는 독립적인 각 키 바인딩, Auto/Skip 조합, Replay 취소 또는 부정 저장 사례의 상세 시험 기록이 아닙니다. 장면별 별도 점수나 인용문, 스크린샷은 제공되지 않았습니다. 사용자가 요청한 추가 수정은 없습니다.

**PROTOTYPE_PLAY_GATE_PASS**는 사용자가 실제 플레이한 현재 S001–S008 QA 프로로그에 한정됩니다. **COMMERCIAL_RELEASE_CLEARANCE_PENDING**: 기계 로프 마커, 별도 표정 그림, 다섯 무음 SFX, 자산 출처와 라이선스, 기존 기술 경고는 최종 상용 승인되지 않았습니다. 이후 이야기 콘텐츠는 수락 범위에 포함되지 않습니다.

QA 실행 파일: C:/Dev/project_all_time_playtest/M10_13/Build/AllTime_M10_13_PlayGate.exe; SHA-256 48EFAB523AA684C653BD1254A6962D3410127B5C02DC1310F6F16F4810666556. Unity 6000.3.21f1 Windows x64 BuildReport는 Succeeded, 오류 0, 경고 486을 기록했습니다. QA 전용 Application.persistentDataPath는 C:/Users/UserK/AppData/LocalLow/ProjectAllTimeQA/AllTime_M10_13_PlayGate이며 SaveData와 MetaProgress는 이 경로에 분리됩니다. 임시 프로젝트 복사본에서 PROLOGUE_S001_START를 설정했고 추적된 생산 Scene은 M2_UI_START를 유지합니다.

M10-12의 전체 EditMode 518 통과 / 실패 0 / 건너뜀 0 및 전체 프로덕션 경로, Save/Load, Replay의 기술 증거는 이전 마일스톤 결과입니다. 이 문서 변경에 대해 새 전체 테스트를 실행한 결과가 아닙니다. M10-14는 M10-13 PR이 main에 실제 병합된 후 시작합니다.

## 장면별 평가표

아래 항목은 검토 기준입니다. 각 장면은 사용자의 전체 플레이 통과 보고 범위에 포함되며 별도 장면 점수나 세부 발언은 제공되지 않았습니다.

| 장면 | 화면 / 인물 | 서사 호흡 | 음악 / 효과음 | 조작 | 문제 / 사용자 의견 | 결정 |
|---|---|---|---|---|---|---|
| S001 가족의 아침 | 주방, 다섯 사람과 보통 로프, 크기와 겹침 | 일상과 대화의 자연스러움 | 아침 BGM 시작 | 대사 진행 / 가독성 | 개별 의견 없음 | COVERED_BY_OVERALL_USER_PASS — NO SCENE-SPECIFIC DEFECT REPORTED |
| S002 미묘한 불편함 | 두 해변 사진, 구도, 반응 | 불편함의 점진적 전달 | 아침 음악 연속, 의자 소리 | 대사 진행 | 개별 의견 없음 | COVERED_BY_OVERALL_USER_PASS — NO SCENE-SPECIFIC DEFECT REPORTED |
| S003 로프와 상담사 | 내면 독백, 발표자, 정보 패널 | 상담사 공개 타이밍 | 웃음 반복, 음악 전환 | 패널 가독성 | 개별 의견 없음 | COVERED_BY_OVERALL_USER_PASS — NO SCENE-SPECIFIC DEFECT REPORTED |
| S004 수납실 | 헌 신발, 새 신발, 가방, 표정 | 침묵과 감정 전환 | 침묵의 효과 | 수동 대기와 해제 | 개별 의견 없음 | COVERED_BY_OVERALL_USER_PASS — NO SCENE-SPECIFIC DEFECT REPORTED |
| S005 작별 | 가족 전원, 배치, 진희 | 작별과 감정의 해소 | 작별 BGM 시작 | 대사 진행 | 개별 의견 없음 | COVERED_BY_OVERALL_USER_PASS — NO SCENE-SPECIFIC DEFECT REPORTED |
| S006 흰 문과 현실 | 국소 흰 문, 흔들림 없음, 두 로프의 구분 | 현실 공개의 효과 | BGM 정지 | 대사 진행 | 개별 의견 없음 | COVERED_BY_OVERALL_USER_PASS — NO SCENE-SPECIFIC DEFECT REPORTED |
| S007 진료 | 의사, 정보 패널, 의료 도표 | 설명의 이해와 호흡 | 대사 중 음량 | 한글 가독성 | 개별 의견 없음 | COVERED_BY_OVERALL_USER_PASS — NO SCENE-SPECIFIC DEFECT REPORTED |
| S008 프로필과 결말 | 세 카드, 초상, 장비, 엔딩 타이틀 | 첫 검토 생각, 하나 선택, 결말 | 장비, 비, 물방울, 버스 브레이크 | 프로필 탐색 / 선택 | 개별 의견 없음 | COVERED_BY_OVERALL_USER_PASS — NO SCENE-SPECIFIC DEFECT REPORTED |

## 실제 입력 체크리스트

사용자는 전반적인 실제 조작에 문제가 없었다고 보고했습니다. 아래의 각 세부 조합을 별도로 시험했다는 보고는 없으므로 개별 체크는 표시하지 않습니다.

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
| 없음 | S001–S008 전체 | 사용자가 문제 없음을 보고 | 전체 플레이, 저장/불러오기 및 재시작 | — | 2026-11-09 사용자 보고 | 추가 수정 요청 없음 | 사용자 전체 경험 통과 보고 | QA 프로로그 승인 |

빌드 식별자와 사용자 보고 결과는 위 수락 항목에 기록합니다. M10-14는 M10-13 병합 이후 시작합니다.

## 과거 기록: 2026-11-09 QA 빌드 진행 및 미완료 확인

아래는 사용자 수락 이전의 당시 상태이며 위 수락 결과로 갱신되었습니다.

- 기준 소스: bdaf1dea39ff2e31ea72ed078cee9a0ea8c6eab6. 별도 임시 프로젝트 복사본에서 Unity 직렬화 API로 시작 노드를 PROLOGUE_S001_START로 설정하고 QA 전용 회사/제품 이름을 사용했습니다. 추적되는 원본 VN_Main과 ProjectSettings는 변경하지 않았습니다.
- Windows x64 BuildReport: Succeeded, 오류 0, 경고 486, 크기 347132612 bytes. 실행 파일 SHA-256: 48EFAB523AA684C653BD1254A6962D3410127B5C02DC1310F6F16F4810666556.
- 실행 파일: C:\Dev\project_all_time_playtest\M10_13\Build\AllTime_M10_13_PlayGate.exe. 시작 시 S001의 첫 한국어 대사와 다음 대사를 확인했습니다. 화면/소리의 주관적 수락은 미실시입니다.
- Player가 보고한 Application.persistentDataPath: C:\Users\UserK\AppData\LocalLow\ProjectAllTimeQA\AllTime_M10_13_PlayGate. 같은 루트의 MetaProgress\meta_progress.json 생성 확인. 일반 게임 데이터는 대상으로 삼지 않았습니다.
- 수동 SaveData 생성, 정확한 문단 Load 및 다음 문단, 종료/재실행 후 보존은 아직 검증되지 않았습니다. 예상 경로는 위 루트의 SaveData이지만 실제 쓰기는 확인하지 못했습니다.
- 실제 Player 입력 확인 중 컴퓨터 사용 도구의 창 활성화가 실패했고 GetCursorPos returned access denied (0x80070005). 창 재선택과 활성화 재시도도 실패했습니다. 이 때문에 직접 수동 저장 화면을 검증할 수 없었습니다. 다른 입력 주입 방식으로 우회하지 않았습니다.
- 현재 Player.log에는 D3D12 info-queue query 0x80004002가 보이며, 확인 시점까지 새 프로젝트 예외는 발견되지 않았습니다. Player 종료 후 로그는 아직 확인하지 못했습니다.
- 아직 PLAYTEST BUILD READY 상태가 아닙니다. 남은 작업: 수동 저장/로드 및 후속 문단, QA 저장 파일과 재실행 보존 확인, 배경/캐릭터/BGM 재생 확인, 종료 후 로그 확인. 사용자 평가 및 최종 PR도 대기 중입니다.

## 과거 기록: 2026-11-09 파일 기반 재확인

아래는 사용자 피드백을 받기 전의 당시 상태이며 위 수락 결과로 갱신되었습니다.

**QA BUILD AVAILABLE — USER SAVE/LOAD VERIFICATION PENDING.** 실행 파일과 Player 데이터가 존재하고 실행 파일 SHA-256은 위 빌드 기록과 일치합니다. 원본 Scene과 PlayerSettings는 기준 커밋과 차이가 없습니다. 기존 M10-12의 518/0/0 EditMode 결과는 이전 마일스톤의 결과이며 이번 문서 변경 후 새 실행 결과가 아닙니다.

QA SaveData 폴더에서 schema-v2 JSON 및 JPEG 쌍을 읽기 전용으로 확인했습니다. 수동 슬롯 00, 01, 03에는 각각 S001-014, S001-021, S002-042가 기록되어 있고, 자동 슬롯 00~04에는 S004-001부터 S008-017의 경계가 기록되어 있습니다. 이는 해당 파일이 QA 경로에 존재한다는 증거입니다. 누가 저장했는지, Load가 정확히 복원했는지, 올바른 후속 문단이 나왔는지, 재실행 후 사용자가 확인했는지는 이 파일만으로 판단하지 않습니다. QA MetaProgress 파일도 존재합니다. 어떤 QA 파일도 초기화하거나 변경하지 않았습니다.

확인 시 QA Player 프로세스가 종료되어 있었습니다. 런처 로그와 QA 기본 Player.log의 완성된 종료 구간에는 프로젝트 예외가 보이지 않았고, D3D12 정보 큐 조회 실패 0x80004002와 종료 시 소유자 미확인 ComputeBuffer 정리 경고가 있었습니다. 경고를 무해하다고 판단하거나 메모리 누수 부재를 주장하지 않습니다.

남은 사용자 실행 확인: S001 진행과 배경/인물/아침 BGM, 빈 슬롯 수동 저장, 여러 문단 진행 후 정확한 Load와 다음 문단, 정상 종료와 재실행 후 슬롯 보존. 사용자 본인의 S001–S008 화면, 호흡, 소리, 실제 조작 및 임시 콘텐츠 결정은 계속 AWAITING_USER_REVIEW입니다. 기존 파일의 존재를 사용자 의견이나 수락으로 처리하지 않습니다. 최종 PR은 사용자 평가와 승인된 수정 이후에만 생성합니다.
