let auth = JSON.parse(localStorage.getItem('auth'));
let withAuth = loginUrl !== "null";

if (withAuth && auth === null) {
    renderLoginView();
} else {
    console.log(baseUrl);
    get(baseUrl, function (res) {
        renderToolbar(withAuth);
        renderCards(res.cards);
        renderTables(res.tables);
        renderCharts(res.charts);
    }, withAuth);
}

function renderCards(cards) {
    const statCardTpl = Handlebars.compile($("#statCardTpl").html());

    cards.forEach(card => {
        const id = card.title.replace(' ', '_');

        $('#gi-container').append(statCardTpl({id: id, title: card.title, value: card.value}));

        if (card.data !== null) {
            const linkElem = $(`#${id}`);
            linkElem.addClass("transition duration-300 hover:bg-gray-200 cursor-pointer");
            linkElem.click(() => {
                if (card.type === "Link") {
                    window.open(card.data, "_blank");
                } else {
                    get(`${baseUrl}/cards/${card.title}`, function (res) {
                        blankTab(JSON.stringify(res, null, 1));
                    }, true);
                }
            });
        }
    });
}

function renderTables(tables) {
    tables.filter(t => t !== null && t.rows.length > 0).forEach(table => {
        const tableTpl = Handlebars.compile($("#tableTpl").html());
        const id = `${table.title.replaceAll(" ", "_")}`;

        // Add headers
        let fullHeaders = "";
        for (const property in table.rows[0]) {
            fullHeaders += `<th class="px-4 py-2">${property.toUpperCase()}</th>`;
        }

        // Add rows
        let rows = "";
        table.rows.forEach((row, i) => {
            let columns = "";
            for (const prop in row) {
                columns += `<td class="px-4 py-3">${row[prop]}</td>`;
            }

            const rowId = row["rowId"];
            rows += `<tr id="${rowId}" class="hover:bg-gray-100 border-b border-gray-200 cursor-pointer">${columns}</tr>`

            $(document).on("click", `#${rowId}`, function () {
                get(`${baseUrl}/tables/${table.title}/${rowId}`, function (res) {
                    blankTab(JSON.stringify(res, null, 1));
                }, true);
            });
        });

        $('#container').append(tableTpl({id: id, title: table.title, headers: fullHeaders, rows: rows, searchId: `search-${id}`}));

        // Search
        $(`#search-${id}`).on("keyup", function () {
            const value = $(this).val().toLowerCase();
            $(`#${id} tbody tr`).filter(function () {
                $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
            });
        });
    });
}

function renderCharts(charts) {
    const chartTpl = Handlebars.compile($("#chartTpl").html());

    charts.forEach(chart => {
        const id = chart.title.replace(' ', '_');
        $('#charts-container').append(chartTpl({id: id, title: chart.title}));
        createChart(chart.type, id, chart.data);
    });
}

function createChart(type, id, data) {
    type = type.toLowerCase();

    let elements = [];
    let chartData = {};
    if (type === 'time') {
        const timeLabel = Object.keys(data[0]).find(x => x !== 'label');
        data.forEach(o => {
            delete Object.assign(o, {['x']: o['label']})['label'];
            delete Object.assign(o, {['y']: o[timeLabel]})[timeLabel];
        });

        elements.push({
            label: timeLabel,
            data: data,
            pointRadius: 0,
            lineTension: 0,
            borderWidth: 2,
            borderColor: "#a8e0ba",
            fill: true,
            backgroundColor: 'rgba(0, 219, 117, 0.2)',
            borderColor: "#a8e0ba",
        });
    } else {
        for (const property in data[0]) {
            chartData[property] = data.map(x => x[property]);
        }

        for (const property in chartData) {
            if (property === 'label') continue;

            elements.push({
                label: property,
                data: chartData[property],
                pointRadius: 0,
                lineTension: 0,
                borderWidth: 2,
                borderColor: "#a8e0ba",
                fill: type === 'line' ? false : true,
                backgroundColor: 'rgba(0, 219, 117, 0.2)',
                borderColor: (type === 'pie' || type === 'doughnut') && chartData.label.length < 10 ? [] : "#a8e0ba",
            });
        }
    }

    const ctx = document.getElementById(id).getContext('2d');
    new Chart(ctx, {
        type: type === 'area' || type === 'time' ? 'line' : type,
        data: {
            labels: type !== 'time' ? chartData.label : null,
            datasets: elements,
        },
        options: {
            scales: type === 'time' ? {
                xAxes: [{
                    type: 'time',
                    distribution: 'series',
                    time: {
                        tooltipFormat: 'YYYY-MM-DD HH:mm',
                        displayFormats: {
                            millisecond: 'HH:mm:ss.SSS',
                            second: 'HH:mm:ss',
                            minute: 'HH:mm',
                            hour: 'HH'
                        },
                    }
                }],
                yAxes: [{
                    gridLines: {
                        drawBorder: false
                    },
                }]
            } : null,
            legend: {
                display: false,
                position: 'bottom'
            },
            tooltips: {
                intersect: false,
                displayColors: false,
                mode: 'index',
            },
        }
    });
}

function renderToolbar() {
    if (!withAuth) return;

    const toolbarTpl = Handlebars.compile($("#toolbarTpl").html());

    $(document).ready(() => {
        $("#logout").click(() => {
            localStorage.clear();
            window.location.reload();
        });
    });

    $('#toolbar-container').append(toolbarTpl());
}

function renderLoginView(withAuth) {
    const loginViewTpl = Handlebars.compile($("#loginViewTpl").html());

    $(document).ready(() => {
        $("#login").click(() => {
            const errMsgElement = $("#error");
            errMsgElement.text("");

            post(loginUrl, {
                email: $("#email").val(),
                password: $("#password").val(),
            }, function (res) {
                try {
                    localStorage.setItem("auth", JSON.stringify({
                        accessToken: res.accessToken,
                        refreshToken: res.refreshToken,
                        expires: JSON.parse(atob(res.accessToken.split('.')[1])).exp,
                    }));
                    window.location.reload();
                } catch (e) {
                    console.error("Failed to parse access token", e);
                }
            }, function (xhr, status, error) {
                errMsgElement.text(error);
            });
        });
    });

    $('body').prepend(loginViewTpl());
}

// UTILS
function post(url, data, onSuccess, onError) {
    return jQuery.ajax({
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        'type': 'POST',
        'url': url,
        'data': JSON.stringify(data),
        'dataType': 'json',
        'cache': false,
        'success': onSuccess,
        'error': onError
    });
}

function get(url, onSuccess) {
    if (withAuth) refreshTokenIfExpired();

    return jQuery.ajax({
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${auth?.accessToken}`
        },
        'cache': false,
        'type': 'GET',
        'url': url,
        'success': onSuccess,
        'error': function (xhr, status, error) {
            console.error(error);
        }
    });
}

function refreshTokenIfExpired() {
    const refreshAt = auth.expires - 2 * 60 * 1000;

    if (Date.now() >= refreshAt) {
        post(refreshUrl, {
            refreshToken: auth.refreshToken,
            expiredAccessToken: auth.accessToken,
        }, function (res) {
            // TODO: move to function
            try {
                auth = {
                    accessToken: res.accessToken,
                    refreshToken: res.refreshToken,
                    expires: JSON.parse(atob(res.accessToken.split('.')[1])).exp,
                };
                localStorage.setItem("auth", JSON.stringify(auth));
                console.log("Token refreshed");
            } catch (e) {
                console.error("Failed to parse access token", e);
            }
        }, function () {
            console.error("Failed to refresh access token");
        });
    }
}

function blankTab(data) {
    let tab = window.open("about:blank", "_blank");
    tab.document.write(`<pre>${data}</pre>`);
    tab.document.close();
}
