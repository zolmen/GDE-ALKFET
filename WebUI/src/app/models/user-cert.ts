export interface UserCert {
    id: string;
    rootCertId: string;
    subjectName: string;
    notBefore: string;
    notAfter: string;
    thumbprint: string;
    createdAt: string;
}

export interface SignUserCert {
    rootCertId: string;
    csrBase64: string;
    validityDays: number;
}
