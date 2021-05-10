
float3 UnpackPosition(int data) {
    int x = (data >> 1) & 0xF;
    int y = (data >> 5) & 0xF;
    int z = (data >> 9) & 0xF;

    return float3(x + 0.5f, y + 0.5f, z + 0.5f);
}

bool UnpackActive(int data) {
    return bool(data & 0x1);
}

int UnpackTypeId(int data) {
    return data >> 13;
}