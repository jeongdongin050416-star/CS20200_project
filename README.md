# CLI Fortress
A command-line Fortress game built with F# / .NET 10.

## AI prompt
Write a command line fortress game using F# (dotnet 10). You should satisfy requirements.md. After you implement it, you should test its behavior conforms requirements.
ai prompt 수정사함
불필요한 test code가 있어서 수정, module별로 파일 나누기
속도 범위가 너무 커서 줄이기

시간범위가 너무 짧으니깐 포탄이 건물을 그냥 통과하거나 상대 탱크를 그냥 통과하는 경우가 있었음
시간 간격을 줄이고 frame간격을 늘려서 해결함

수정사항
기존: 포탄이 User1이나 User2를 넘어가거나 땅에 닿이면 miss로 여기고 다음 user로 넘어감
수정: 포탄이 x방향으로 화면을 벗어나거나 빌딩에 부딪히거나 땅에 닿이면 miss로 여기는 걸로 수정

포탄이 날라가다가 User1을 넘어갔을 때 포탄이 갑자기 사라지는게 자연스럽지 않아서 수정
포탄이 빌딩을 통과하는게 부자연스러워서 수정함