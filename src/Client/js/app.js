var emitter = emitter.connect({
    secure: true
}); 
var key = 'vDY5ft8Ilw3YGK4N2KIfIE80TtaMM7t0';
emitter.on('connect', function(){
    // once we're connected, subscribe to the 'tweet-stream' channel
    console.log('emitter: connected');
    emitter.subscribe({ key: key, channel: "dashboard-updates/counts" });
    emitter.subscribe({ key: key, channel: "dashboard-updates/history" });
});

Chart.defaults.global.legend.position = 'bottom'

// Add a doughnut chart
var tweetCountsChart = new Chart(document.getElementById("tweetCounts"), {
    type: 'doughnut',
    data: { labels: ["Tweets", "Retweets"], datasets: [{ 
        data: [50, 50],
        backgroundColor: [ "#1abc9c", "#3498db" ],
        hoverBackgroundColor: [ "#16a085", "#2980b9" ]
    }]},
});


var tweetHistoryChart = new Chart(document.getElementById("tweetHistory"), {
    type: 'line',
    data: {
        labels: ["5s ago", "4s ago", "3s ago", "2s ago", "1s ago", "Just Now"],
        datasets: [
            {
                label: "Number of Tweets",
                backgroundColor: [ "#f1c40f" ],
                borderColor: [ "#f39c12" ],
                tension: 0.1,
                borderWidth: 1,
                data: [0, 0, 0, 0, 0, 0],
            }
        ]    
    },
    options: {
        scales: { yAxes: [{ ticks: { max: 40, min: 0, stepSize: 10} }]}
    }
});

// on every message, print it out
emitter.on('message', function(msg){
    var update = msg.asObject();
    console.log(msg.channel)
    switch(msg.channel){
        // If we've received a counter update
        case 'dashboard-updates/counts/':
            var total = update.original + update.retweets;
            var original = (update.original) / total;
            var retweets = (update.retweets) / total;
            tweetCountsChart.data.datasets[0].data[0] = original * 100;
            tweetCountsChart.data.datasets[0].data[1] = retweets * 100;
            tweetCountsChart.update();
        break;

        // If we've received a history update
        case 'dashboard-updates/history/':
            tweetHistoryChart.data.datasets[0].data = update;
            tweetHistoryChart.update();
            console.log(update);
        break;
    }
});