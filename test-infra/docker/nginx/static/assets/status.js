async function fetchStatus() {
    try {
        const response = await fetch('/status-raw');
        if (!response.ok) throw new Error('Network response was not ok');
        const text = await response.text();
        
        const lines = text.split('\n');
        const active = lines[0].split(':')[1].trim();
        const [accepted, handled, requests] = lines[2].trim().split(/\s+/);
        const stateLine = lines[3].trim().split(' ');
        const reading = stateLine[1];
        const writing = stateLine[3];
        const waiting = stateLine[5];

        document.getElementById('active-conns').textContent = active;
        document.getElementById('accepted').textContent = accepted;
        document.getElementById('handled').textContent = handled;
        document.getElementById('requests').textContent = requests;
        
        document.getElementById('reading').textContent = reading;
        document.getElementById('writing').textContent = writing;
        document.getElementById('waiting').textContent = waiting;

    } catch (error) {
        console.error('Error fetching status:', error);
    }
}

fetchStatus();
setInterval(fetchStatus, 2000);