const express = require('express');
const http = require('http');
const { Server } = require('socket.io');
const cors = require('cors');

const app = express();
app.use(cors());

const server = http.createServer(app);
const io = new Server(server, {
    cors: {
        origin: "*",
        methods: ["GET", "POST"]
    }
});

let voiceChannels = ["Общий", "Игровой", "Музыка"];

// ====================== SOCKET.IO ======================

io.on('connection', (socket) => {
    console.log(`Клиент подключился: ${socket.id}`);

    // Отправляем текущие каналы при подключении
    socket.emit('update_channels', { voice: voiceChannels });

    // Создание нового канала
    socket.on('create_channel', (data) => {
        if (data.name && !voiceChannels.includes(data.name)) {
            voiceChannels.push(data.name);
            console.log(`Создан канал: ${data.name}`);
            
            // Отправляем обновление всем клиентам
            io.emit('update_channels', { voice: voiceChannels });
        }
    });

    // Присоединение к каналу
    socket.on('join_channel', (data) => {
        console.log(`Пользователь ${socket.id} присоединился к каналу: ${data.channel}`);
        socket.join(data.channel);
    });

    // P2P сигналы (для будущего WebRTC)
    socket.on('p2p_signal', (data) => {
        socket.to(data.target).emit('p2p_signal', data);
    });

    socket.on('disconnect', () => {
        console.log(`Клиент отключился: ${socket.id}`);
    });
});

// ====================== START SERVER ======================

const PORT = process.env.PORT || 3000;
server.listen(PORT, () => {
    console.log(`Сервер запущен на порту ${PORT}`);
    console.log('Доступные каналы:', voiceChannels);
});