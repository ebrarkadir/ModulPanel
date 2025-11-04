document.getElementById("submitBtn").addEventListener("click", function () {
    let score = 0;
    const answers = new FormData(document.getElementById("quizForm"));

    for (let value of answers.values()) {
        score += Number(value);
    }

    document.getElementById("result").innerHTML =
        `🎯 Puanınız: ${score} / 3`;
});
